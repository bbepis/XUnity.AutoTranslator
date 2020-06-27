using System;
using System.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.IL2CPP.Text
{
   internal class TMP_TextComponent : ITextComponent
   {
      internal static IntPtr __set_text;
      internal static IntPtr __get_text;
      internal static IntPtr __get_richText;
      internal static IntPtr __TeshMeshPro_OnEnable;
      internal static IntPtr __TeshMeshProUGUI_OnEnable;

      static TMP_TextComponent()
      {
         if( UnityTypes.TMP_Text != null )
         {
            __set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text.ClassPointer, "set_text", typeof( void ), typeof( string ) );
            __get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text.ClassPointer, "get_text", typeof( string ) );
            __get_richText = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text.ClassPointer, "get_richText", typeof( bool ) );
         }
         if( UnityTypes.TextMeshPro != null )
         {
            __TeshMeshPro_OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMeshPro.ClassPointer, "OnEnable", typeof( void ) );
         }
         if( UnityTypes.TextMeshProUGUI != null )
         {
            __TeshMeshPro_OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMeshProUGUI.ClassPointer, "OnEnable", typeof( void ) );
         }
      }

      private IntPtr _ptr;
      private Component _component;
      private uint _gcHandle;

      public TMP_TextComponent( IntPtr ptr )
      {
         _ptr = ptr;
         _component = new Component( ptr );
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( _component );
      }

      public TMP_TextComponent( Component component )
      {
         _ptr = component.Pointer;
         _component = component;
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( _component );
      }

      public string text
      {
         get
         {
            var ptr = Il2CppUtilities.InvokeMethod( __get_text, _ptr );
            return UnhollowerBaseLib.IL2CPP.Il2CppStringToManaged( ptr );
         }
         set
         {
            var ptr = UnhollowerBaseLib.IL2CPP.ManagedStringToIl2Cpp( value );
            Il2CppUtilities.InvokeMethod( __set_text, _ptr, ptr );
         }
      }

      public GameObject GameObject => _component.gameObject;

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
         return -1407380504 + _ptr.GetHashCode();
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
            __get_richText,
            _ptr,
            (void**)param,
            ref exc );
         Il2CppException.RaiseExceptionIfNecessary( exc );
         return *(bool*)(long)UnhollowerBaseLib.IL2CPP.il2cpp_object_unbox( obj );
      }
   }
}
