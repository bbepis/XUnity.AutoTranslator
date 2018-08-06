using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   public static class UtageHooks
   {
      public static readonly Type[] All = new[] {
         typeof( AdvCommand_ParseCellLocalizedTextHook ),
         typeof( AdvEngine_JumpScenario ),
      };
   }

   [Harmony]
   public static class AdvCommand_ParseCellLocalizedTextHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.Types.AdvCommand != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.Types.AdvCommand, "ParseCellLocalizedText", new Type[] { } );
      }

      static void Postfix( object __instance, ref string __result )
      {
         var result = AutoTranslationPlugin.Current.Hook_TextChanged_WithResult( __instance, __result );
         if( !string.IsNullOrEmpty( result ) )
         {
            __result = result;
         }
      }
   }

   [Harmony]
   public static class AdvEngine_JumpScenario
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.Types.AdvEngine != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.Types.AdvEngine, "JumpScenario", new Type[] { typeof( string ) } );
      }

      static void Prefix( ref string label )
      {
         if( AutoTranslationPlugin.Current.TryGetReverseTranslation( label, out string key ) )
         {
            label = key;
         }
      }
   }
}
