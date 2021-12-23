#if IL2CPP

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   internal static class UGUIHooks
   {
      public static readonly Type[] All = new[] {
         typeof( Text_text_Hook ),
         typeof( Text_OnEnable_Hook ),
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Text_Methods.set_text != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.Text_Methods.set_text;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Text?.ProxyType, "text" ).GetSetMethod();
      }

      static void Postfix( Component __instance )
      {
         //__instance = (Component)Il2CppUtilities.CreateProxyComponentWithProxyType( __instance.Pointer, UnityTypes.Text.ProxyType );

         var instance = new TextComponent( __instance );

         _Postfix( instance );
      }

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static void ML_Detour( IntPtr instance, IntPtr value )
      {
         var __instance = new TextComponent( instance );

         Il2CppUtilities.InvokeMethod( UnityTypes.Text_Methods.set_text, instance, value );

         _Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class Text_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Text_Methods.OnEnable != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.Text_Methods.OnEnable;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.Text?.ProxyType, "OnEnable" );
      }

      static void Postfix( Component __instance )
      {
         var instance = new TextComponent( __instance );

         _Postfix( instance );
      }

      static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
      }

      static void ML_Detour( IntPtr __instance )
      {
         var instance = new TextComponent( __instance );

         Il2CppUtilities.InvokeMethod( UnityTypes.Text_Methods.OnEnable, __instance );

         _Postfix( instance );
      }
   }
}

#endif
