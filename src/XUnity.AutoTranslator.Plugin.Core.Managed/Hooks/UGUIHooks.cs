using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI
{
   internal static class UGUIHooks
   {
      public static readonly Type[] All = new[] {
         typeof( Text_text_Hook ),
         typeof( Text_OnEnable_Hook ),
         typeof( TextArea2D_text_Hook ),
         typeof( TextArea2D_TextData_Hook ),
         typeof( TextData_ctor_Hook ),
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextArea2D_text_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TextArea2D != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( ClrTypes.TextArea2D, "text" )?.GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

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
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextArea2D_TextData_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TextArea2D != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( ClrTypes.TextArea2D, "TextData" )?.GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static Action<object, object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, object>>();
      }

      static void MM_Detour( object __instance, object value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextData_ctor_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.TextData != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return ClrTypes.TextData.GetConstructor( new[] { typeof( string ) } );
      }

      static void Postfix( object __instance, string text )
      {
         __instance.SetExtensionData( text );
      }

      static Action<object, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, string>>();
      }

      static void MM_Detour( object __instance, string text )
      {
         _original( __instance, text );

         Postfix( __instance, text );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Text, "text" )?.GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

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
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class Text_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.Text, "OnEnable" );
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
      }

      static Action<object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object>>();
      }

      static void MM_Detour( object __instance )
      {
         _original( __instance );

         Postfix( __instance );
      }
   }
}
