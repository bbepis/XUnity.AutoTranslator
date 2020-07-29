using System;
using System.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.IL2CPP.Text
{
   internal class TMP_TextComponent : ITextComponent
   {
      private IntPtr _ptr;
      private Component _component;
      private uint _gcHandle;

      public TMP_TextComponent( IntPtr ptr )
      {
         _component = Il2CppUtilities.CreateProxyComponent( ptr );
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( _component );
         _ptr = ptr;
      }

      public TMP_TextComponent( Component component )
      {
         _ptr = Il2CppUtilities.GetIl2CppInstancePointer( component );
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( component );
         _component = component;
      }

      public string Text
      {
         get
         {
            var ptr = Il2CppUtilities.InvokeMethod( UnityTypes.TMP_Text_Methods.get_text, _ptr );
            return UnhollowerBaseLib.IL2CPP.Il2CppStringToManaged( ptr );
         }
         set
         {
            var ptr = UnhollowerBaseLib.IL2CPP.ManagedStringToIl2Cpp( value );
            Il2CppUtilities.InvokeMethod( UnityTypes.TMP_Text_Methods.set_text, _ptr, ptr );
         }
      }

      public bool IsPlaceholder()
      {
         if( UnityTypes.TMP_InputField_Methods.get_placeholder == IntPtr.Zero ) return false;

         var inputField = _component.gameObject.GetFirstComponentInSelfOrAncestor( UnityTypes.TMP_InputField.Il2CppType );
         if( inputField == null ) return false;

         var ptr = Il2CppUtilities.GetIl2CppInstancePointer( inputField );
         var placeholderPtr = Il2CppUtilities.InvokeMethod( UnityTypes.TMP_InputField_Methods.get_placeholder, ptr );
         return placeholderPtr == _ptr;
      }

      public Component Component => _component;

      public int GetScope()
      {
         return _component.gameObject.scene.buildIndex;
      }

      public bool IsSpammingComponent()
      {
         return false;
      }

      public override bool Equals( object obj )
      {
         return obj is TMP_TextComponent component &&
                 EqualityComparer<IntPtr>.Default.Equals( _ptr, component._ptr );
      }

      public override int GetHashCode()
      {
         return _ptr.ToInt32();
      }

      public bool IsCollected()
      {
         return UnhollowerBaseLib.IL2CPP.il2cpp_gchandle_get_target( _gcHandle ) == IntPtr.Zero;
      }

      public unsafe bool SupportsRichText()
      {
         System.IntPtr* param = null;
         System.IntPtr exc = default;
         System.IntPtr obj = UnhollowerBaseLib.IL2CPP.il2cpp_runtime_invoke(
            UnityTypes.TMP_Text_Methods.get_richText,
            _ptr,
            (void**)param,
            ref exc );
         Il2CppException.RaiseExceptionIfNecessary( exc );
         return *(bool*)(long)UnhollowerBaseLib.IL2CPP.il2cpp_object_unbox( obj );
      }
   }
}
