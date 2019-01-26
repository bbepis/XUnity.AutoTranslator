using System;
using System.Reflection;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   public static class TextGetterCompatHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextPropertyGetterHook1 ),
         typeof( TextPropertyGetterHook2 ),
      };
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class TextPropertyGetterHook1
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
         TextGetterCompatMode.ReplaceTextWithOriginal( __instance, ref __result );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class TextPropertyGetterHook2
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
         TextGetterCompatMode.ReplaceTextWithOriginal( __instance, ref __result );
      }
   }
}
