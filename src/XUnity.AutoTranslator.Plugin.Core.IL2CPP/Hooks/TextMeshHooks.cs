using System;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.IL2CPP.Text;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   internal static class TextMeshHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextMesh_text_Hook ),
         //typeof( GameObject_SetActive_Hook )
      };
   }

   internal static class TextMesh_text_Hook
   {
      static bool Prepare( object instance )
      {
         return TextMeshComponent.__set_text != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return TextMeshComponent.__set_text;
      }

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static void ML_Detour( IntPtr __instance, IntPtr value )
      {
         var instance = new TextMeshComponent( __instance );

         Il2CppUtilities.InvokeMethod( TextMeshComponent.__set_text, __instance, value );

         _Postfix( instance );
      }
   }

   //internal static class GameObject_SetActive_Hook
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return true;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessToolsShim.Method( typeof( GameObject ), "SetActive", typeof( bool ) );
   //   }

   //   static void Postfix( GameObject __instance, bool active )
   //   {
   //      if( !TextMeshHooks.HooksOverriden )
   //      {
   //         if( active )
   //         {
   //            var tms = __instance.GetComponentsInternal(( UnityTypes.TextMesh.Type );
   //            foreach( var tm in tms )
   //            {
   //               AutoTranslationPlugin.Current.Hook_TextChanged( tm, true );
   //            }
   //         }
   //      }
   //   }

   //   static void MM_Detour( IntPtr instance, IntPtr value )
   //   {
   //      var __instance = new GameObject( instance );
   //      var v = Il2CppUtilities.PointerToManagedBool( value );


   //      _original( __instance, value );

   //      Postfix( __instance, v );
   //   }
   //}
}
