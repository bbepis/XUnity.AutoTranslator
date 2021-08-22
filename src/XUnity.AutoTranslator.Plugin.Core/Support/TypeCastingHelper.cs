using XUnity.Common.Utilities;
using System;

#if IL2CPP
using UnhollowerBaseLib;
#endif

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   internal static class TypeCastingHelper
   {
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
               throw new ArgumentException( $"{typeof( TObject )} is not al Il2Cpp reference type" );
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
               castedObject = (TObject)UnhollowerBaseLib.Runtime.ClassInjectorBase.GetMonoObjectFromIl2CppPointer( instancePointer );
               return castedObject != null;
            }

            castedObject = Il2CppUtilities.Factory<TObject>.CreateProxyComponent( instancePointer );
            return castedObject != null;
         }
#endif

         castedObject = default;
         return false;
      }
   }
}
