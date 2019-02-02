using System;
using System.Reflection;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextGetterCompat
{
   internal static class TextGetterCompatHooks
   {
      public static readonly Type[] All = new[] {
         typeof( Text_text_Hook ),
         typeof( TMP_Text_text_Hook ),
      };
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   internal static class Text_text_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         var text = AccessTools.Property( ClrTypes.Text, "text" );
         return text.GetGetMethod();
      }

      static void Postfix( object __instance, ref string __result )
      {
         TextGetterCompatModeHelper.ReplaceTextWithOriginal( __instance, ref __result );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   internal static class TMP_Text_text_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.TMP_Text, "text" ).GetGetMethod();
      }

      static void Postfix( object __instance, ref string __result )
      {
         TextGetterCompatModeHelper.ReplaceTextWithOriginal( __instance, ref __result );
      }
   }
}
