using Il2CppSystem.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.IL2CPP.Text
{
   internal class TextComponent : ITextComponent
   {
      internal static IntPtr __set_text;
      internal static IntPtr __get_text;
      internal static IntPtr __OnEnable;
      internal static IntPtr __get_supportRichText;

      static TextComponent()
      {
         if( UnityTypes.Text != null )
         {
            __set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text.ClassPointer, "set_text", typeof( void ), typeof( string ) );
            __get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text.ClassPointer, "get_text", typeof( string ) );
            __OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text.ClassPointer, "OnEnable", typeof( void ) );
            __get_supportRichText = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text.ClassPointer, "get_supportRichText", typeof( bool ) );
         }
      }

      private IntPtr _ptr;
      private Component _component;
      private uint _gcHandle;

      public TextComponent( IntPtr ptr )
      {
         _ptr = ptr;
         _component = new Component( ptr );
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( _component );
      }

      public TextComponent( Component component )
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
         return obj is TextComponent component &&
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
            __get_supportRichText,
            _ptr,
            (void**)param,
            ref exc );
         Il2CppException.RaiseExceptionIfNecessary( exc );
         return *(bool*)(long)UnhollowerBaseLib.IL2CPP.il2cpp_object_unbox( obj );
      }
   }
}
