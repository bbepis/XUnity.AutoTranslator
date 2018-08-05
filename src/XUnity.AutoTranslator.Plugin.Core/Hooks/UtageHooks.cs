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
         typeof( AdvCommand_ParseCellLocalizedTextHook )
      };
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
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
         var result = AutoTranslationPlugin.Current.Override_TextChanged( __instance, __result );
         if( !string.IsNullOrEmpty( result ) )
         {
            __result = result;
         }
      }
   }
}
