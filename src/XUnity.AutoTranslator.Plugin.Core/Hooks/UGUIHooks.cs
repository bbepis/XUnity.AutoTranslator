using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   public static class UGUIHooks
   {
      public static bool HooksOverriden = false;

      public static readonly Type[] All = new[] {
         typeof( TextPropertyHook ),
         typeof( OnEnableHook ),
      };
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class TextPropertyHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Types.Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         var text = AccessTools.Property( Types.Text, "text" );
         return text.GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         if( !UGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleFont( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class OnEnableHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Types.Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         var OnEnable = AccessTools.Method( Types.Text, "OnEnable" );
         return OnEnable;
      }

      static void Postfix( object __instance )
      {
         if( !UGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleFont( __instance );
      }
   }
}
