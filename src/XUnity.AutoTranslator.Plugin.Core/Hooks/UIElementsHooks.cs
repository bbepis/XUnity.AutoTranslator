using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UIElements
{
   internal static class UIElementsHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextElement_text_Hook ),
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextElement_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextElement != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextElement?.ClrType, "text" )?.GetSetMethod();
      }

#if MANAGED
      static void Postfix( object __instance )
#else
      static void Postfix( Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase __instance )
#endif
      {
#if IL2CPP
         __instance = Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextElement.ClrType );
#endif

         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      static Action<object, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, string>>();
      }

      static void MM_Detour( object __instance, string value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
   }
}
