using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   static class VirtcallRuleBreaker
   {
      public static Delegate GenerateDelegate( MethodBase method, bool useCallVirt )
      {
         var parameterTypes = GetActualParameters( method );
         var actionParameters = GetActionParameters( method );
         var returnType = method is MethodInfo mi ? mi.ReturnType : typeof( void );

         var dynamicMethod = new DynamicMethodDefinition( "DynamicMethod_" + method.Name, returnType, actionParameters );
         
         var il = dynamicMethod.GetILProcessor();
         if( actionParameters.Length >= 1 )
         {
            il.Emit( Mono.Cecil.Cil.OpCodes.Ldarg_0 );
            il.Emit( Mono.Cecil.Cil.OpCodes.Castclass, parameterTypes[ 0 ] );
            if( actionParameters.Length >= 2 )
            {
               il.Emit( Mono.Cecil.Cil.OpCodes.Ldarg_1 );
               il.Emit( Mono.Cecil.Cil.OpCodes.Castclass, parameterTypes[ 1 ] );
               if( actionParameters.Length >= 3 )
               {
                  il.Emit( Mono.Cecil.Cil.OpCodes.Ldarg_2 );
                  il.Emit( Mono.Cecil.Cil.OpCodes.Castclass, parameterTypes[ 2 ] );
                  if( actionParameters.Length >= 4 )
                  {
                     il.Emit( Mono.Cecil.Cil.OpCodes.Ldarg_3 );
                     il.Emit( Mono.Cecil.Cil.OpCodes.Castclass, parameterTypes[ 3 ] );
                  }
               }
            }
         }
         il.Emit( useCallVirt ? Mono.Cecil.Cil.OpCodes.Callvirt : Mono.Cecil.Cil.OpCodes.Call, method );
         il.Emit( Mono.Cecil.Cil.OpCodes.Ret );

         var delegateType = returnType == typeof( void ) ? GetGenericActionType( actionParameters ) : GetGenericFuncType( actionParameters, returnType );
         var actionOrFunc = dynamicMethod.Generate().CreateDelegate( delegateType );
         return actionOrFunc;
      }

      static Type[] GetActionParameters( MethodBase method )
      {
         if( method.IsStatic )
         {
            return method.GetParameters().Select( x => typeof( object ) ).ToArray();
         }
         else
         {
            return new Type[] { typeof( object ) }.Concat( method.GetParameters().Select( x => typeof( object ) ) ).ToArray();
         }
      }

      static Type[] GetActualParameters( MethodBase method )
      {
         if( method.IsStatic )
         {
            return method.GetParameters().Select( x => x.ParameterType ).ToArray();
         }
         else
         {
            return new Type[] { method.DeclaringType }.Concat( method.GetParameters().Select( x => x.ParameterType ) ).ToArray();
         }
      }

      static Type GetGenericActionType( Type[] parameterTypes )
      {
         return GetActionType( parameterTypes.Length ).MakeGenericType( parameterTypes );
      }

      static Type GetGenericFuncType( Type[] parameterTypes, Type returnType )
      {
         return GetFuncType( parameterTypes.Length ).MakeGenericType( parameterTypes.Concat( new[] { returnType } ).ToArray() );
      }

      static Type GetActionType( int parameterCount )
      {
         switch( parameterCount )
         {
            case 0:
               return typeof( Action );
            case 1:
               return typeof( Action<> );
            case 2:
               return typeof( Action<,> );
            case 3:
               return typeof( Action<,,> );
            case 4:
               return typeof( Action<,,,> );
            default:
               throw new ArgumentException( nameof( parameterCount ) );
         }
      }

      static Type GetFuncType( int parameterCount )
      {
         switch( parameterCount )
         {
            case 0:
               return typeof( Func<> );
            case 1:
               return typeof( Func<,> );
            case 2:
               return typeof( Func<,,> );
            case 3:
               return typeof( Func<,,,> );
            case 4:
               return typeof( Func<,,,,> );
            default:
               throw new ArgumentException( nameof( parameterCount ) );
         }
      }

      //static string GetFuncTypeName( int parameterCount )
      //{
      //   return "System.Func`" + ( parameterCount + 1 );
      //}

      //static string GetActionTypeName( int parameterCount )
      //{
      //   if( parameterCount == 0 )
      //   {
      //      return "System.Action";
      //   }
      //   else
      //   {
      //      return "System.Action`" + parameterCount;
      //   }
      //}
   }
}
