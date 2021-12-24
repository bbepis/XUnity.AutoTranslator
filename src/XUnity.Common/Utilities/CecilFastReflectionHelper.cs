using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// Mostly based on:
   ///  * https://github.com/MonoMod/MonoMod/blob/master/MonoMod.Utils/FastReflectionHelper.cs
   ///  * https://github.com/pardeike/Harmony/blob/master/Harmony/Extras/FastAccess.cs
   /// </summary>
   internal static class CecilFastReflectionHelper
   {
      private static readonly Type[] DynamicMethodDelegateArgs = { typeof( object ), typeof( object[] ) };

      public static FastReflectionDelegate CreateFastDelegate( MethodBase method, bool directBoxValueAccess, bool forceNonVirtcall )
      {
         DynamicMethodDefinition dmd = new DynamicMethodDefinition( $"FastReflection<{method.DeclaringType.FullName + "." + method.Name}>", typeof( object ), DynamicMethodDelegateArgs );
         ILProcessor il = dmd.GetILProcessor();

         ParameterInfo[] args = method.GetParameters();

         bool generateLocalBoxValuePtr = true;

         if( !method.IsStatic )
         {
            il.Emit( OpCodes.Ldarg_0 );
            if( method.DeclaringType.IsValueType )
            {
               il.Emit( OpCodes.Unbox_Any, method.DeclaringType );
            }
         }

         for( int i = 0; i < args.Length; i++ )
         {
            Type argType = args[ i ].ParameterType;
            bool argIsByRef = argType.IsByRef;
            if( argIsByRef )
               argType = argType.GetElementType();
            bool argIsValueType = argType.IsValueType;

            if( argIsByRef && argIsValueType && !directBoxValueAccess )
            {
               // Used later when storing back the reference to the new box in the array.
               il.Emit( OpCodes.Ldarg_1 );
               il.Emit( OpCodes.Ldc_I4, i );
            }

            il.Emit( OpCodes.Ldarg_1 );
            il.Emit( OpCodes.Ldc_I4, i );

            if( argIsByRef && !argIsValueType )
            {
               il.Emit( OpCodes.Ldelema, typeof( object ) );
            }
            else
            {
               il.Emit( OpCodes.Ldelem_Ref );
               if( argIsValueType )
               {
                  if( !argIsByRef || !directBoxValueAccess )
                  {
                     // if !directBoxValueAccess, create a new box if required
                     il.Emit( OpCodes.Unbox_Any, argType );
                     if( argIsByRef )
                     {
                        // box back
                        il.Emit( OpCodes.Box, argType );

                        // store new box value address to local 0
                        il.Emit( OpCodes.Dup );
                        il.Emit( OpCodes.Unbox, argType );
                        if( generateLocalBoxValuePtr )
                        {
                           generateLocalBoxValuePtr = false;
                           dmd.Definition.Body.Variables.Add( new VariableDefinition( new PinnedType( new PointerType( dmd.Definition.Module.TypeSystem.Void ) ) ) );
                        }
                        il.Emit( OpCodes.Stloc_0 );

                        // arr and index set up already
                        il.Emit( OpCodes.Stelem_Ref );

                        // load address back to stack
                        il.Emit( OpCodes.Ldloc_0 );
                     }
                  }
                  else
                  {
                     // if directBoxValueAccess, emit unbox (get value address)
                     il.Emit( OpCodes.Unbox, argType );
                  }
               }
            }
         }

         if( method.IsConstructor )
         {
            il.Emit( OpCodes.Newobj, method as ConstructorInfo );
         }
         else if( method.IsFinal || !method.IsVirtual || forceNonVirtcall )
         {
            il.Emit( OpCodes.Call, method as MethodInfo );
         }
         else
         {
            il.Emit( OpCodes.Callvirt, method as MethodInfo );
         }

         Type returnType = method.IsConstructor ? method.DeclaringType : ( method as MethodInfo ).ReturnType;
         if( returnType != typeof( void ) )
         {
            if( returnType.IsValueType )
            {
               il.Emit( OpCodes.Box, returnType );
            }
         }
         else
         {
            il.Emit( OpCodes.Ldnull );
         }

         il.Emit( OpCodes.Ret );

         return (FastReflectionDelegate)dmd.Generate().CreateDelegate( typeof( FastReflectionDelegate ) );
      }

      public static Func<T, F> CreateFastFieldGetter<T, F>( FieldInfo fieldInfo )
      {
         if( fieldInfo == null )
            throw new ArgumentNullException( nameof( fieldInfo ) );
         if( !typeof( F ).IsAssignableFrom( fieldInfo.FieldType ) )
            throw new ArgumentException( "FieldInfo type does not match return type." );
         if( typeof( T ) != typeof( object ) )
            if( fieldInfo.DeclaringType == null || !fieldInfo.DeclaringType.IsAssignableFrom( typeof( T ) ) )
               throw new MissingFieldException( typeof( T ).Name, fieldInfo.Name );

         var name = $"FastReflection<{typeof( T ).FullName}.Get_{fieldInfo.Name}>";

         var dm = new DynamicMethodDefinition( name, typeof( F ), new[] { typeof( T ) } );

         var il = dm.GetILProcessor();
         if( !fieldInfo.IsStatic )
         {
            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Castclass, fieldInfo.DeclaringType );
         }

         il.Emit( fieldInfo.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldInfo );
         if( fieldInfo.FieldType.IsValueType != typeof( F ).IsValueType )
         {
            il.Emit( OpCodes.Box, fieldInfo.FieldType );
         }
         il.Emit( OpCodes.Ret );

         return (Func<T, F>)dm.Generate().CreateDelegate( typeof( Func<T, F> ) );
      }

      public static Action<T, F> CreateFastFieldSetter<T, F>( FieldInfo fieldInfo )
      {
         if( fieldInfo == null )
            throw new ArgumentNullException( nameof( fieldInfo ) );
         if( !typeof( F ).IsAssignableFrom( fieldInfo.FieldType ) )
            throw new ArgumentException( "FieldInfo type does not match argument type." );
         if( typeof( T ) != typeof( object ) )
            if( fieldInfo.DeclaringType == null || !fieldInfo.DeclaringType.IsAssignableFrom( typeof( T ) ) )
               throw new MissingFieldException( typeof( T ).Name, fieldInfo.Name );

         var name = $"FastReflection<{typeof( T ).FullName}.Set_{fieldInfo.Name}>";

         var dm = new DynamicMethodDefinition( name, null, new[] { typeof( T ), typeof( F ) } );

         var il = dm.GetILProcessor();
         if( !fieldInfo.IsStatic )
         {
            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Castclass, fieldInfo.DeclaringType );
         }

         il.Emit( OpCodes.Ldarg_1 );
         if( fieldInfo.FieldType == typeof( F ) )
         {

         }
         else if( fieldInfo.FieldType.IsValueType != typeof( F ).IsValueType )
         {
            if( fieldInfo.FieldType.IsValueType )
            {
               il.Emit( OpCodes.Unbox_Any, fieldInfo.FieldType );
            }
            else
            {
               il.Emit( OpCodes.Box, fieldInfo.FieldType );
            }
         }
         else
         {
            il.Emit( OpCodes.Castclass, fieldInfo.FieldType );
         }

         il.Emit( fieldInfo.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldInfo );
         il.Emit( OpCodes.Ret );

         return (Action<T, F>)dm.Generate().CreateDelegate( typeof( Action<T, F> ) );
      }
   }
}
