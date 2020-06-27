using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.IL2CPP.Text;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro
{
   internal static class TextMeshProHooks
   {
      public static bool HooksOverriden = false;

      public static readonly Type[] All = new[] {
         typeof( TeshMeshProUGUI_OnEnable_Hook ),
         typeof( TeshMeshPro_OnEnable_Hook ),
         typeof( TMP_Text_text_Hook ),
      };
   }

   internal static class TeshMeshProUGUI_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return TMP_TextComponent.__TeshMeshProUGUI_OnEnable != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return TMP_TextComponent.__TeshMeshProUGUI_OnEnable;
      }

      static void Postfix( ITextComponent __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr instance )
      {
         var __instance = new TMP_TextComponent( instance );

         Il2CppUtilities.InvokeMethod( TMP_TextComponent.__TeshMeshProUGUI_OnEnable, instance );

         Postfix( __instance );
      }
   }

   internal static class TeshMeshPro_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return TMP_TextComponent.__TeshMeshPro_OnEnable != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return TMP_TextComponent.__TeshMeshProUGUI_OnEnable;
      }

      static void Postfix( ITextComponent __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr instance )
      {
         var __instance = new TMP_TextComponent( instance );

         Il2CppUtilities.InvokeMethod( TMP_TextComponent.__TeshMeshPro_OnEnable, instance );

         Postfix( __instance );
      }
   }

   internal static class TMP_Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return TMP_TextComponent.__set_text != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return TMP_TextComponent.__set_text;
      }

      static void Postfix( ITextComponent __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr instance, IntPtr value )
      {
         var __instance = new TMP_TextComponent( instance );

         Il2CppUtilities.InvokeMethod( TMP_TextComponent.__set_text, instance, value );

         Postfix( __instance );
      }
   }
}
