using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Constants;

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
         return text.GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         if( !UGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   internal static class Text_OnEnable_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         var OnEnable = AccessTools.Method( ClrTypes.Text, "OnEnable" );
         return OnEnable;
      }

      static void Postfix( object __instance )
      {
         if( !UGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }
}
