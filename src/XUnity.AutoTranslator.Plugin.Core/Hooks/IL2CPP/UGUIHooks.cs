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
         __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.Text.ProxyType );

         _Postfix( __instance );
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static void ML_Detour( IntPtr instance, IntPtr value )
      {
         var __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( instance, UnityTypes.Text.ProxyType );

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
         if( __instance.Pointer.IsInstancePointerAssignableFrom( UnityTypes.Text.ClassPointer ) )
         {
            __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.Text.ProxyType );
            _Postfix( __instance );
         }
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
      }

      static void ML_Detour( IntPtr instance )
      {
         Il2CppUtilities.InvokeMethod( UnityTypes.Text_Methods.OnEnable, instance );

         if( instance.IsInstancePointerAssignableFrom( UnityTypes.Text.ClassPointer ) )
         {
            var __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( instance, UnityTypes.Text.ProxyType );
            _Postfix( __instance );
         }
      }
   }
}

#endif
