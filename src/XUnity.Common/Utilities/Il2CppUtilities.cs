#if IL2CPP
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;
using XUnity.Common.Logging;

namespace XUnity.Common.Utilities
{
   public static class Il2CppUtilities
   {
      private static Dictionary<string, IntPtr> ourImagesMap;
      private static Dictionary<Type, Func<IntPtr, Il2CppObjectBase>> FactoryFunctionsByType = new Dictionary<Type, Func<IntPtr, Il2CppObjectBase>>();

      public static class Factory<TComponent>
      {
         public static readonly Func<IntPtr, TComponent> CreateProxyComponent =
            (Func<IntPtr, TComponent>)ExpressionHelper.CreateTypedFastInvoke(
               typeof( TComponent ).GetConstructor(
                  BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic,
                  null,
                  new Type[] { typeof( IntPtr ) },
                  null
               ) );
      }

      public static Il2CppObjectBase CreateProxyComponentWithDerivedType( IntPtr ptr, Type type )
      {
         if( !FactoryFunctionsByType.TryGetValue( type, out var factoryFunction ) )
         {
            var fn = typeof( Factory<> ).MakeGenericType( type ).GetField( "CreateProxyComponent", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ).GetValue( null );
            factoryFunction = (Func<IntPtr, Il2CppObjectBase>)fn;
            FactoryFunctionsByType[ type ] = factoryFunction;
         }

         return factoryFunction( ptr );
      }

      public static Func<object, uint> GetGarbageCollectionHandle =
         CustomFastReflectionHelper.CreateFastFieldGetter<object, uint>(
            typeof( Il2CppObjectBase ).GetField(
               "myGcHandle",
               BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) );

      public static readonly Func<IntPtr, Component> CreateProxyComponent =
         (Func<IntPtr, Component>)ExpressionHelper.CreateTypedFastInvoke(
            typeof( Component ).GetConstructor(
               BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic,
               null,
               new Type[] { typeof( IntPtr ) },
               null
            ) );

      public static IntPtr GetIl2CppInstancePointer( object obj )
      {
         var gcHandle = GetGarbageCollectionHandle( obj );
         var intPtr = IL2CPP.il2cpp_gchandle_get_target( gcHandle );
         if( intPtr == IntPtr.Zero )
         {
            throw new ObjectCollectedException( "Object was garbage collected in IL2CPP domain" );
         }
         return intPtr;
      }

      public static IntPtr GetIl2CppInstancePointer( uint gcHandle )
      {
         var intPtr = IL2CPP.il2cpp_gchandle_get_target( gcHandle );
         if( intPtr == IntPtr.Zero )
         {
            throw new ObjectCollectedException( "Object was garbage collected in IL2CPP domain" );
         }
         return intPtr;
      }

      public static IntPtr GetIl2CppClass( string namespaze, string className )
      {
         if( ourImagesMap == null )
         {
            ourImagesMap = (Dictionary<string, IntPtr>)typeof( IL2CPP ).GetField( "ourImagesMap", BindingFlags.NonPublic | BindingFlags.Static ).GetValue( null );
         }

         foreach( var image in ourImagesMap.Values )
         {
            var clazz = IL2CPP.il2cpp_class_from_name( image, namespaze, className );

            if( clazz != IntPtr.Zero )
               return clazz;
         }

         return IntPtr.Zero;
      }

      public static IntPtr GetIl2CppMethod( IntPtr? clazz, string methodName, Type returnType, params Type[] types )
      {
         try
         {
            if( !clazz.HasValue || clazz == IntPtr.Zero ) return IntPtr.Zero;

            return IL2CPP.GetIl2CppMethod( clazz.Value, false, methodName, returnType.FullName, types.Select( x => x.FullName ).ToArray() );
         }
         catch
         {
            return IntPtr.Zero;
         }
      }

      public static IntPtr GetIl2CppMethod( IntPtr? clazz, string methodName, string returnType, params string[] types )
      {
         if( !clazz.HasValue || clazz == IntPtr.Zero ) return IntPtr.Zero;

         return IL2CPP.GetIl2CppMethod( clazz.Value, false, methodName, returnType, types );
      }

      unsafe public static IntPtr InvokeMethod( IntPtr method, IntPtr obj, params IntPtr[] paramtbl )
      {
         if( method == IntPtr.Zero )
            return IntPtr.Zero;




         if( paramtbl == null || paramtbl.Length == 0 )
         {
            IntPtr* parameters = null;
            IntPtr exp = IntPtr.Zero;
            var returnval = IL2CPP.il2cpp_runtime_invoke( method, obj, (void**)parameters, ref exp );
            Il2CppException.RaiseExceptionIfNecessary( exp );
            return returnval;
         }
         else
         {
            fixed( IntPtr* ptr = &paramtbl[ 0 ] )
            {
               IntPtr exp = IntPtr.Zero;
               var returnval = IL2CPP.il2cpp_runtime_invoke( method, obj, (void**)ptr, ref exp );
               Il2CppException.RaiseExceptionIfNecessary( exp );
               return returnval;
            }
         }




         //var cnt = paramtbl?.Length ?? 0;
         //var parameters = stackalloc IntPtr[ cnt ];
         //for( int i = 0; i < cnt; i++ )
         //{
         //   parameters[ i ] = paramtbl[ i ];
         //}

         //IntPtr exp = IntPtr.Zero;
         //var returnval = UnhollowerBaseLib.IL2CPP.il2cpp_runtime_invoke( method, obj, (void**)parameters, ref exp );
         //Il2CppException.RaiseExceptionIfNecessary( exp );
         //return returnval;





         //IntPtr[] intPtrArray;
         //IntPtr returnval = IntPtr.Zero;
         //intPtrArray = ( ( paramtbl != null ) ? paramtbl : new IntPtr[ 0 ] );
         //IntPtr intPtr = Marshal.AllocHGlobal( intPtrArray.Length * sizeof( void* ) );
         //try
         //{
         //   void** pointerArray = (void**)intPtr.ToPointer();
         //   for( int i = 0; i < intPtrArray.Length; i++ )
         //      pointerArray[ i ] = intPtrArray[ i ].ToPointer();
         //   IntPtr exp = IntPtr.Zero;
         //   returnval = UnhollowerBaseLib.IL2CPP.il2cpp_runtime_invoke( method, obj, pointerArray, ref exp );
         //   Il2CppException.RaiseExceptionIfNecessary( exp );
         //}
         //finally
         //{
         //   Marshal.FreeHGlobal( intPtr );
         //}
         //return returnval;
      }

      unsafe public static bool PointerToManagedBool( IntPtr ptr )
      {
         return *(bool*)(long)IL2CPP.il2cpp_object_unbox( ptr );
      }
   }
}

#endif
