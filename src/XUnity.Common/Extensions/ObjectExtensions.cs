#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

#if IL2CPP
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
#endif


namespace XUnity.Common.Extensions
{
   public static class ObjectExtensions
   {
#if IL2CPP
      private static readonly IntPtr GetIl2CppType;

      static ObjectExtensions()
      {
         GetIl2CppType = IL2CPP.GetIl2CppMethod(
               Il2CppClassPointerStore<Il2CppSystem.Object>.NativeClassPtr,
               false,
               "GetType",
               IL2CPP.RenderTypeName<Type>() );

      }

      public unsafe static Il2CppSystem.Type GetIl2CppTypeSafe( this object that )
      {
         if( that is Il2CppObjectBase obj )
         {
            IL2CPP.Il2CppObjectBaseToPtrNotNull( obj );
            System.IntPtr* param = null;
            System.IntPtr exc = IntPtr.Zero;
            System.IntPtr intPtr = IL2CPP.il2cpp_runtime_invoke(
               GetIl2CppType,
               IL2CPP.Il2CppObjectBaseToPtrNotNull( obj ),
               (void**)param,
               ref exc );
            Il2CppInterop.Runtime.Il2CppException.RaiseExceptionIfNecessary( exc );
            return intPtr != IntPtr.Zero ? new Il2CppSystem.Type( intPtr ) : null;
         }
         return null;
      }

      public static bool IsCollected( this Il2CppObjectBase that )
      {
         var gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( that );
         return IL2CPP.il2cpp_gchandle_get_target( gcHandle ) == IntPtr.Zero;
      }

      public static Il2CppSystem.Type GetUnityType( this object obj )
      {
         return obj.GetIl2CppTypeSafe();
      }
#else
      public static Type GetUnityType( this object obj )
      {
         return obj.GetType();
      }
#endif

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="TObject"></typeparam>
      /// <param name="obj"></param>
      /// <param name="castedObject"></param>
      /// <returns></returns>
      public static bool TryCastTo<TObject>( this object obj, out TObject castedObject )
      {
         if( obj is TObject c )
         {
            castedObject = c;
            return true;
         }

#if IL2CPP
         if( obj is Il2CppObjectBase il2cppObject )
         {
            IntPtr nativeClassPtr = Il2CppClassPointerStore<TObject>.NativeClassPtr;
            if( nativeClassPtr == IntPtr.Zero )
            {
               throw new ArgumentException( $"{typeof( TObject )} is not an Il2Cpp reference type" );
            }

            var instancePointer = il2cppObject.Pointer;
            IntPtr intPtr = IL2CPP.il2cpp_object_get_class( instancePointer );
            if( !IL2CPP.il2cpp_class_is_assignable_from( nativeClassPtr, intPtr ) )
            {
               castedObject = default;
               return false;
            }
            if( RuntimeSpecificsStore.IsInjected( intPtr ) )
            {
               castedObject = (TObject)ClassInjectorBase.GetMonoObjectFromIl2CppPointer( instancePointer );
               return castedObject != null;
            }

            castedObject = Il2CppUtilities.Factory<TObject>.CreateProxyComponent( instancePointer );
            return castedObject != null;
         }
#endif

         castedObject = default;
         return false;
      }

#if IL2CPP
      public static bool IsInstancePointerAssignableFrom( this IntPtr instancePointer, IntPtr classPointer )
      {
         IntPtr intPtr = IL2CPP.il2cpp_object_get_class( instancePointer );
         return IL2CPP.il2cpp_class_is_assignable_from( classPointer, intPtr );
      }
#endif
   }
}

