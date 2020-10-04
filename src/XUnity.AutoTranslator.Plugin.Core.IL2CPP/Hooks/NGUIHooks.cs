using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.IL2CPP.Extensions;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI
{
   internal static class NGUIHooks
   {
      public static readonly Type[] All = new[] {
         typeof( UILabel_text_Hook ),
         typeof( UILabel_OnEnable_Hook )
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class UILabel_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UILabel_Methods.set_text != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.UILabel_Methods.set_text;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UILabel?.ProxyType, "text" )?.GetSetMethod();
      }

      static void Postfix( Component __instance )
      {
         var instance = new UILabelComponent( __instance );

         _Postfix( instance );
      }

      public static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static void ML_Detour( IntPtr instance, IntPtr value )
      {
         var __instance = new UILabelComponent( instance );

         Il2CppUtilities.InvokeMethod( UnityTypes.UILabel_Methods.set_text, instance, value );

         _Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class UILabel_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UIRect_Methods.OnEnable != IntPtr.Zero;
      }

      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.UIRect_Methods.OnEnable;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.UIRect?.ProxyType, "OnEnable" );
      }

      static void Postfix( Component __instance )
      {
         if( UnityTypes.UILabel.Il2CppType.IsAssignableFrom( __instance.GetIl2CppTypeSafe() ) )
         {
            var instance = new UILabelComponent( __instance );

            _Postfix( instance );
         }
      }

      public static void _Postfix( ITextComponent __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
      }

      static void ML_Detour( IntPtr instance )
      {
         var component = Il2CppUtilities.CreateProxyComponent( instance );
         if( UnityTypes.UILabel.Il2CppType.IsAssignableFrom( component.GetIl2CppTypeSafe() ) )
         {
            var __instance = new UILabelComponent( component );

            Il2CppUtilities.InvokeMethod( UnityTypes.UIRect_Methods.OnEnable, instance );

            _Postfix( __instance );
         }
      }
   }
}
