#if IL2CPP

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

namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   internal class UILabelComponent : ITextComponent
   {
      private IntPtr _ptr;
      private Component _component;
      private uint _gcHandle;

      public UILabelComponent( IntPtr ptr )
      {
         _component = Il2CppUtilities.CreateProxyComponent( ptr );
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( _component );
         _ptr = ptr;
      }

      public UILabelComponent( Component component )
      {
         _ptr = Il2CppUtilities.GetIl2CppInstancePointer( component );
         _gcHandle = Il2CppUtilities.GetGarbageCollectionHandle( component );
         _component = component;
      }

      public string Text
      {
         get
         {
            var ptr = Il2CppUtilities.InvokeMethod( UnityTypes.UILabel_Methods.get_text, _ptr );
            return UnhollowerBaseLib.IL2CPP.Il2CppStringToManaged( ptr );
         }
         set
         {
            var ptr = UnhollowerBaseLib.IL2CPP.ManagedStringToIl2Cpp( value );
            Il2CppUtilities.InvokeMethod( UnityTypes.UILabel_Methods.set_text, _ptr, ptr );
         }
      }

      public bool IsPlaceholder()
      {
         var inputField = _component.gameObject.GetFirstComponentInSelfOrAncestor( UnityTypes.UIInput.Il2CppType );
         return inputField != null;
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
         return obj is UILabelComponent component &&
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
         return false;
      }
   }
}

#endif
