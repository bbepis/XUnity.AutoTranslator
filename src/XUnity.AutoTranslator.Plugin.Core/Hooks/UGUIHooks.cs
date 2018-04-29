using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   public delegate void UGUITextChanged( object graphic );
   public delegate void UGUITextAwakened( object graphic );

   public static class UGUIHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextPropertyHook ),
         typeof( OnEnableHook ),
      };

      public static event UGUITextChanged TextChanged;
      public static event UGUITextAwakened TextAwakened;

      public static void FireTextAwakened( object graphic )
      {
         TextAwakened?.Invoke( graphic );
      }

      public static void FireTextChanged( object graphic )
      {
         TextChanged?.Invoke( graphic );
      }
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
         UGUIHooks.FireTextChanged( __instance );
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
         UGUIHooks.FireTextAwakened( __instance );
      }
   }
}
