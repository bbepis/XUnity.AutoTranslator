#if IL2CPP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro
{
   internal static class TextMeshProHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TeshMeshProUGUI_OnEnable_Hook ),
         typeof( TeshMeshPro_OnEnable_Hook ),
         typeof( TMP_Text_text_Hook ),
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TeshMeshProUGUI_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextMeshProUGUI_Methods.OnEnable != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.TextMeshProUGUI_Methods.OnEnable;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TextMeshProUGUI?.ProxyType, "OnEnable" );
      }

      static void Postfix( Component __instance )
      {
         __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextMeshProUGUI.ProxyType );

         _Postfix( __instance );
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
      }

      static void ML_Detour( IntPtr instance )
      {
         Il2CppUtilities.InvokeMethod( UnityTypes.TextMeshProUGUI_Methods.OnEnable, instance );

         var __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( instance, UnityTypes.TextMeshProUGUI.ProxyType );
         _Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TeshMeshPro_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextMeshPro_Methods.OnEnable != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.TextMeshPro_Methods.OnEnable;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TextMeshPro?.ProxyType, "OnEnable" );
      }

      static void Postfix( Component __instance )
      {
         __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextMeshPro.ProxyType );

         _Postfix( __instance );
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
      }

      static void ML_Detour( IntPtr instance )
      {
         Il2CppUtilities.InvokeMethod( UnityTypes.TextMeshPro_Methods.OnEnable, instance );

         var __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( instance, UnityTypes.TextMeshPro.ProxyType );
         _Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text_Methods.set_text != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.TMP_Text_Methods.set_text;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TMP_Text?.ProxyType, "text" ).GetSetMethod();
      }

      static void Postfix( Component __instance )
      {
         __instance = __instance.CreateTextMeshProDerivedProxy();

         _Postfix( __instance );
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static void ML_Detour( IntPtr instance, IntPtr value )
      {
         Il2CppUtilities.InvokeMethod( UnityTypes.TMP_Text_Methods.set_text, instance, value );

         var __instance = instance.CreateTextMeshProDerivedProxy();
         _Postfix( __instance );
      }
   }
}

#endif
