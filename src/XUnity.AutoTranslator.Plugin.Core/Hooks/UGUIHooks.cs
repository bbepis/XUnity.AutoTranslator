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

   [Harmony, HarmonyPriority( Priority.Last )]
   internal static class Text_text_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.Text, "text" )?.GetSetMethod();
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

   [Harmony, HarmonyPriority( Priority.Last )]
   internal static class Text_OnEnable_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.Text, "OnEnable" );
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
