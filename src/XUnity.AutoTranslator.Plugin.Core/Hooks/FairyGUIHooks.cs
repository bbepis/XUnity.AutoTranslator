using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class FairyGUIHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextField_text_Hook ),
         typeof( TextField_htmlText_Hook ),
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextField_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextField != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextField?.ClrType, "text" )?.GetSetMethod();
      }

#if MANAGED
      static void Postfix( object __instance )
#else
      static void Postfix(Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase __instance )
#endif
      {
#if IL2CPP
         __instance = Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextField.ClrType );
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

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextField_htmlText_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextField != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextField?.ClrType, "htmlText" )?.GetSetMethod();
      }


#if MANAGED
      static void Postfix( object __instance )
#else
      static void Postfix(Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase __instance )
#endif
      {
#if IL2CPP
         __instance = Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextField.ClrType );
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
