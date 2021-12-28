#if MANAGED

using System;
using System.Reflection;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
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
         return UnityTypes.Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Text.ClrType, "text" )?.GetGetMethod();
      }

      static void Postfix( object __instance, ref string __result )
      {
         TextGetterCompatModeHelper.ReplaceTextWithOriginal( __instance, ref __result );
      }

      static Func<object, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<object, string>>();
      }

      static string MM_Detour( object __instance )
      {
         var result = _original( __instance );

         Postfix( __instance, ref result );

         return result;
      }
   }
   
   internal static class TMP_Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TMP_Text.ClrType, "text" )?.GetGetMethod();
      }

      static void Postfix( object __instance, ref string __result )
      {
         TextGetterCompatModeHelper.ReplaceTextWithOriginal( __instance, ref __result );
      }

      static Func<object, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<object, string>>();
      }

      static string MM_Detour( object __instance )
      {
         var result = _original( __instance );

         Postfix( __instance, ref result );

         return result;
      }
   }
}

#endif
