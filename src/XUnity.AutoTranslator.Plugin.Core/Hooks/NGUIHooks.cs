using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.RuntimeHooker.Core;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI
{
   internal static class NGUIHooks
   {
      public static bool HooksOverriden = false;

      public static readonly Type[] All = new[] {
         typeof( UILabel_text_Hook ),
         typeof( UILabel_OnEnable_Hook )
      };
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class UILabel_text_Hook
   {
      static bool Prepare( object instance )
      {
         return Constants.ClrTypes.UILabel != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( Constants.ClrTypes.UILabel, "text" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         if( !NGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class UILabel_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return Constants.ClrTypes.UILabel != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( Constants.ClrTypes.UILabel, "OnEnable" );
      }

      public static void Postfix( object __instance )
      {
         if( !NGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }
}
