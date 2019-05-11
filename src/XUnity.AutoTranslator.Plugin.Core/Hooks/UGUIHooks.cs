using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.RuntimeHooker.Core;

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

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( ClrTypes.Text, "text" )?.GetSetMethod();
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

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class Text_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.Text, "OnEnable" );
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
