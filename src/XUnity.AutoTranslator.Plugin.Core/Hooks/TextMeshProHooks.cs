using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro
{
   public static class TextMeshProHooks
   {
      public static bool HooksOverriden = false;

      public static readonly Type[] All = new[] {
         typeof( TeshMeshProUGUIOnEnableHook ),
         typeof( TeshMeshProOnEnableHook ),
         typeof( TextPropertyHook ),
         typeof( SetTextHook1 ),
         typeof( SetTextHook2 ),
         typeof( SetTextHook3 ),
         typeof( SetCharArrayHook1 ),
         typeof( SetCharArrayHook2 ),
         typeof( SetCharArrayHook3 ),
      };
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class TeshMeshProUGUIOnEnableHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TextMeshProUGUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.TextMeshProUGUI, "OnEnable" );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class TeshMeshProOnEnableHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TextMeshPro != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.TextMeshPro, "OnEnable" );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class TextPropertyHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.TMP_Text, "text" ).GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class SetTextHook1
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.TMP_Text, "SetText", new[] { typeof( StringBuilder ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class SetTextHook2
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.TMP_Text, "SetText", new[] { typeof( string ), typeof( bool ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class SetTextHook3
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.TMP_Text, "SetText", new[] { typeof( string ), typeof( float ), typeof( float ), typeof( float ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class SetCharArrayHook1
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.TMP_Text, "SetCharArray", new[] { typeof( char[] ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class SetCharArrayHook2
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.TMP_Text, "SetCharArray", new[] { typeof( char[] ), typeof( int ), typeof( int ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class SetCharArrayHook3
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.TMP_Text, "SetCharArray", new[] { typeof( int[] ), typeof( int ), typeof( int ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }
}
