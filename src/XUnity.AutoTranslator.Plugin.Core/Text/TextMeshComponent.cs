#if IL2CPP

using System;
using System.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   internal class TextMeshComponent : ITextComponent
   {
      private IntPtr _ptr;
      private Component _component;
      private uint _gcHandle;

      public TextMeshComponent( IntPtr ptr )
      {
         _component = Il2CppUtilities.CreateProxyComponent( ptr );
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( _component );
         _ptr = ptr;
      }

      public TextMeshComponent( Component component )
      {
         _ptr = Il2CppUtilities.GetIl2CppInstancePointer( component );
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( component );
         _component = component;
      }

      public string Text
      {
         get
         {
            var ptr = Il2CppUtilities.InvokeMethod( UnityTypes.TextMesh_Methods.get_text, _ptr );
            return UnhollowerBaseLib.IL2CPP.Il2CppStringToManaged( ptr );
         }
         set
         {
            var ptr = UnhollowerBaseLib.IL2CPP.ManagedStringToIl2Cpp( value );
            Il2CppUtilities.InvokeMethod( UnityTypes.TextMesh_Methods.set_text, _ptr, ptr );
         }
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
         return obj is TextMeshComponent component &&
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
            UnityTypes.TextMesh_Methods.get_richText,
            _ptr,
            (void**)param,
            ref exc );
         Il2CppException.RaiseExceptionIfNecessary( exc );
         return *(bool*)(long)UnhollowerBaseLib.IL2CPP.il2cpp_object_unbox( obj );
      }

      public bool IsPlaceholder()
      {
         return false;
      }
   }
}

#endif
