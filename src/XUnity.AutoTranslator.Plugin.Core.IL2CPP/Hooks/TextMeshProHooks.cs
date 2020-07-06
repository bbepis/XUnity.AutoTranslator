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

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr __instance )
      {
         var instance = new TMP_TextComponent( __instance );

         Il2CppUtilities.InvokeMethod( TMP_TextComponent.__TeshMeshProUGUI_OnEnable, __instance );

         _Postfix( instance );
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

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr __instance )
      {
         var instance = new TMP_TextComponent( __instance );

         Il2CppUtilities.InvokeMethod( TMP_TextComponent.__TeshMeshPro_OnEnable, __instance );

         _Postfix( instance );
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

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static void ML_Detour( IntPtr __instance, IntPtr value )
      {
         var instance = new TMP_TextComponent( __instance );

         Il2CppUtilities.InvokeMethod( TMP_TextComponent.__set_text, __instance, value );

         _Postfix( instance );
      }
   }
}
