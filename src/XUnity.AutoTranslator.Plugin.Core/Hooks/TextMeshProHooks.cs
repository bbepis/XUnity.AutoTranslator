using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.RuntimeHooker.Core;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro
{
   internal static class TextMeshProHooks
   {
      public static bool HooksOverriden = false;

      public static readonly Type[] All = new[] {
         typeof( TeshMeshProUGUI_OnEnable_Hook ),
         typeof( TeshMeshPro_OnEnable_Hook ),
         typeof( TMP_Text_text_Hook ),
         typeof( TMP_Text_SetText_Hook1 ),
         typeof( TMP_Text_SetText_Hook2 ),
         typeof( TMP_Text_SetText_Hook3 ),
         typeof( TMP_Text_SetCharArray_Hook1 ),
         typeof( TMP_Text_SetCharArray_Hook2 ),
         typeof( TMP_Text_SetCharArray_Hook3 ),
      };
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TeshMeshProUGUI_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TextMeshProUGUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.TextMeshProUGUI, "OnEnable" );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TeshMeshPro_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TextMeshPro != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.TextMeshPro, "OnEnable" );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TMP_Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( ClrTypes.TMP_Text, "text" )?.GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }

      //static void Prefix( object __instance, ref string value )
      //{
      //   if( !TextMeshProHooks.HooksOverriden )
      //   {
      //      var result = AutoTranslationPlugin.Current.Hook_TextChanged_WithResult( __instance, value, false );
      //      if( result != null )
      //      {
      //         value = result;
      //      }
      //   }
      //   AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      //}
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TMP_Text_SetText_Hook1
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.TMP_Text, "SetText", new[] { typeof( StringBuilder ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TMP_Text_SetText_Hook2
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.TMP_Text, "SetText", new[] { typeof( string ), typeof( bool ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TMP_Text_SetText_Hook3
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.TMP_Text, "SetText", new[] { typeof( string ), typeof( float ), typeof( float ), typeof( float ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TMP_Text_SetCharArray_Hook1
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.TMP_Text, "SetCharArray", new[] { typeof( char[] ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TMP_Text_SetCharArray_Hook2
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.TMP_Text, "SetCharArray", new[] { typeof( char[] ), typeof( int ), typeof( int ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }

   [HarmonyPriorityShim( HookPriority.Last )]
   internal static class TMP_Text_SetCharArray_Hook3
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.TMP_Text, "SetCharArray", new[] { typeof( int[] ), typeof( int ), typeof( int ) } );
      }

      static void Postfix( object __instance )
      {
         if( !TextMeshProHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
         }
         AutoTranslationPlugin.Current.Hook_HandleComponent( __instance );
      }
   }
}
