using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.IL2CPP.Text;
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   internal static class UGUIHooks
   {
      public static readonly Type[] All = new[] {
         typeof( Text_text_Hook ),
         typeof( Text_get_text_Hook ),
         typeof( Text_OnEnable_Hook ),
      };
   }

   internal static class Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return TextComponent.__set_text != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return TextComponent.__set_text;
      }

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr instance, IntPtr value )
      {
         var __instance = new TextComponent( instance );

         Il2CppUtilities.InvokeMethod( TextComponent.__set_text, instance, value );

         _Postfix( __instance );
      }
   }

   internal static class Text_get_text_Hook
   {
      static bool Prepare( object instance )
      {
         return TextComponent.__get_text != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return TextComponent.__get_text;
      }

      static string _Postfix( ITextComponent __instance )
      {
         var result = AutoTranslationPlugin.Current.Hook_TextChanged_WithResult( __instance, null, false );
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );

         return result;
      }

      static IntPtr ML_Detour( IntPtr instance )
      {
         var __instance = new TextComponent( instance );

         var result = Il2CppUtilities.InvokeMethod( TextComponent.__get_text, instance );

         var stringResult = _Postfix( __instance );

         if(stringResult != null )
         {
            return UnhollowerBaseLib.IL2CPP.ManagedStringToIl2Cpp( stringResult );
         }
         
         return result;
      }
   }

   internal static class Text_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return TextComponent.__OnEnable != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return TextComponent.__OnEnable;
      }

      static MethodBase TargetMethod( object instance )
      {
         if( UnityTypes.Text?.ProxyType != null )
         {
            return AccessToolsShim.Method( UnityTypes.Text.ProxyType, "OnEnable" );
         }
         return null;
      }

      static void Postfix( Component __instance )
      {
         var instance = new TextComponent( __instance );

         _Postfix( instance );
      }

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr __instance )
      {
         var instance = new TextComponent( __instance );

         Il2CppUtilities.InvokeMethod( TextComponent.__OnEnable, __instance );

         _Postfix( instance );
      }
   }
}
