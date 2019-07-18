using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

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
         return ClrTypes.UILabel != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( ClrTypes.UILabel, "text" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         if( !NGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static Action<object, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, string>>();
      }

      static void MM_Detour( object __instance, string value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class UILabel_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.UILabel != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.UILabel, "OnEnable" );
      }

      public static void Postfix( object __instance )
      {
         if( !NGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      static Action<object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object>>();
      }

      static void MM_Detour( object __instance )
      {
         _original( __instance );

         Postfix( __instance );
      }
   }
}
