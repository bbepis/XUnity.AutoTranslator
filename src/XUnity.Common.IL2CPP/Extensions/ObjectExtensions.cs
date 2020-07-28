using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using XUnity.Common.Constants;

namespace XUnity.Common.IL2CPP.Extensions
{
   public static class ObjectExtensions
   {
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
         var obj = (Il2CppObjectBase)that;

         UnhollowerBaseLib.IL2CPP.Il2CppObjectBaseToPtrNotNull( obj );
         System.IntPtr* param = null;
         System.IntPtr exc = IntPtr.Zero;
         System.IntPtr intPtr = UnhollowerBaseLib.IL2CPP.il2cpp_runtime_invoke(
            GetIl2CppType,
            UnhollowerBaseLib.IL2CPP.Il2CppObjectBaseToPtrNotNull( obj ),
            (void**)param,
            ref exc );
         Il2CppException.RaiseExceptionIfNecessary( exc );
         return ( intPtr != (System.IntPtr)0 ) ? new Il2CppSystem.Type( intPtr ) : null;
      }
   }
}
