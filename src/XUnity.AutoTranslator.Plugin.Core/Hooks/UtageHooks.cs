using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.UtageSupport;

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
   public static class AdvEngine_JumpScenario
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
         UtageHelpers.FixLabel( ref label );
      }
   }
}
