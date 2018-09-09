using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI
{
   public static class NGUIHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextPropertyHook ),
         typeof( OnEnableHook )
      };
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class TextPropertyHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.Types.UILabel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( Constants.Types.UILabel, "text" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class OnEnableHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.Types.UILabel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.Types.UILabel, "OnEnable" );
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
      }
   }
}
