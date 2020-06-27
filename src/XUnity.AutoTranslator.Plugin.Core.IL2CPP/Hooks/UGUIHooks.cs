using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
   internal static class UGUIHooks
   {
      public static bool HooksOverriden = false;

      public static readonly Type[] All = new[] {
         typeof( Text_text_Hook ),
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

      static void Postfix( ITextComponent __instance )
      {
         if( !UGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr instance, IntPtr value )
      {
         var __instance = new TextComponent( instance );

         Il2CppUtilities.InvokeMethod( TextComponent.__set_text, instance, value );

         Postfix( __instance );
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

      static void Postfix( ITextComponent __instance )
      {
         if( !UGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr instance )
      {
         var __instance = new TextComponent( instance );

         Il2CppUtilities.InvokeMethod( TextComponent.__OnEnable, instance );

         Postfix( __instance );
      }
   }
}
