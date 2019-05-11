using System;
using System.Reflection;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextGetterCompat
{
   internal static class TextGetterCompatHooks
   {
      public static readonly Type[] All = new[] {
         typeof( Text_text_Hook ),
         typeof( TMP_Text_text_Hook ),
      };
   }
   
   internal static class Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( ClrTypes.Text, "text" )?.GetGetMethod();
      }

      static void Postfix( object __instance, ref string __result )
      {
         TextGetterCompatModeHelper.ReplaceTextWithOriginal( __instance, ref __result );
      }
   }
   
   internal static class TMP_Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( ClrTypes.TMP_Text, "text" )?.GetGetMethod();
      }

      static void Postfix( object __instance, ref string __result )
      {
         TextGetterCompatModeHelper.ReplaceTextWithOriginal( __instance, ref __result );
      }
   }
}
