using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class UtageHooks
   {
      public static readonly Type[] All = new[] {
         typeof( AdvCommand_ParseCellLocalizedText_Hook ),
         typeof( AdvEngine_JumpScenario_Hook ),
      };
   }

   [Harmony]
   internal static class AdvCommand_ParseCellLocalizedText_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.AdvCommand != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.AdvCommand, "ParseCellLocalizedText", new Type[] { } );
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
   internal static class AdvEngine_JumpScenario_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.AdvEngine != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.AdvEngine, "JumpScenario", new Type[] { typeof( string ) } );
      }

      static void Prefix( ref string label )
      {
         UtageHelper.FixLabel( ref label );
      }
   }
}
