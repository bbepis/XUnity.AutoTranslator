#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

#if IL2CPP
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
#endif


namespace XUnity.Common.Extensions
{
   public static class ObjectExtensions
   {
#if IL2CPP
      private static readonly IntPtr GetIl2CppType;

      static ObjectExtensions()
      {
         GetIl2CppType = UnhollowerBaseLib.IL2CPP.GetIl2CppMethod(
               Il2CppClassPointerStore<Il2CppSystem.Object>.NativeClassPtr,
               false,
               "GetType",
               UnhollowerBaseLib.IL2CPP.RenderTypeName<Type>() );

      }

      public unsafe static Il2CppSystem.Type GetIl2CppTypeSafe( this object that )
      {
         if( that is Il2CppObjectBase obj )
         {
            UnhollowerBaseLib.IL2CPP.Il2CppObjectBaseToPtrNotNull( obj );
            System.IntPtr* param = null;
            System.IntPtr exc = IntPtr.Zero;
            System.IntPtr intPtr = UnhollowerBaseLib.IL2CPP.il2cpp_runtime_invoke(
               GetIl2CppType,
               UnhollowerBaseLib.IL2CPP.Il2CppObjectBaseToPtrNotNull( obj ),
               (void**)param,
               ref exc );
            Il2CppException.RaiseExceptionIfNecessary( exc );
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
   }
}

