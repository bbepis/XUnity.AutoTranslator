#if MANAGED

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro
{
   internal static class TextMeshProHooks
   {
      public static readonly Type[] All = new[] {
         typeof( TextWindow_SetText_Hook ),
         typeof( TeshMeshProUGUI_OnEnable_Hook ),
         typeof( TeshMeshProUGUI_text_Hook ),
         typeof( TeshMeshPro_OnEnable_Hook ),
         typeof( TeshMeshPro_text_Hook ),
         typeof( TMP_Text_text_Hook ),
         typeof( TMP_Text_SetText_Hook1 ),
         typeof( TMP_Text_SetText_Hook2 ),
         typeof( TMP_Text_SetText_Hook3 ),
         typeof( TMP_Text_SetCharArray_Hook1 ),
         typeof( TMP_Text_SetCharArray_Hook2 ),
         typeof( TMP_Text_SetCharArray_Hook3 ),
      };

      public static readonly Type[] DisableScrollInTmp = new[] {
         typeof( TMP_Text_maxVisibleCharacters_Hook ),
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TextWindow_SetText_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextWindow != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TextWindow.ClrType, "SetText", new Type[] { typeof( string ) } );
      }

      static void Postfix( object __instance )
      {
         Settings.SetCurText?.Invoke( __instance );
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
   internal static class TeshMeshProUGUI_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextMeshProUGUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TextMeshProUGUI.ClrType, "OnEnable" );
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

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TeshMeshProUGUI_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text == null && UnityTypes.TextMeshProUGUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextMeshProUGUI.ClrType, "text" ).GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
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
   internal static class TeshMeshPro_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TextMeshPro != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TextMeshPro.ClrType, "OnEnable" );
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

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TeshMeshPro_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text == null && UnityTypes.TextMeshPro != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TextMeshPro.ClrType, "text" ).GetSetMethod();
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
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
   internal static class TMP_Text_maxVisibleCharacters_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TMP_Text.ClrType, "maxVisibleCharacters" )?.GetSetMethod();
      }

      static void Prefix( object __instance, ref int value )
      {
         var info = __instance.GetTextTranslationInfo();
         if( info != null && info.IsTranslated && 0 < value )
         {
            value = 99999;
         }
      }

      static Action<object, int> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, int>>();
      }

      static void MM_Detour( object __instance, int value )
      {
         Prefix( __instance, ref value );

         _original( __instance, value );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_text_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.TMP_Text.ClrType, "text" )?.GetSetMethod();
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
   internal static class TMP_Text_SetText_Hook1
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TMP_Text.ClrType, "SetText", new[] { typeof( StringBuilder ) } );
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static Action<object, StringBuilder> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, StringBuilder>>();
      }

      static void MM_Detour( object __instance, StringBuilder value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_SetText_Hook2
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TMP_Text.ClrType, "SetText", new[] { typeof( string ), typeof( bool ) } );
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static Action<object, string, bool> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, string, bool>>();
      }

      static void MM_Detour( object __instance, string value, bool arg2 )
      {
         _original( __instance, value, arg2 );

         Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_SetText_Hook3
   {
      delegate void OriginalMethod( object arg1, string arg2, float arg3, float arg4, float arg5 );

      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TMP_Text.ClrType, "SetText", new[] { typeof( string ), typeof( float ), typeof( float ), typeof( float ) } );
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( object __instance, string value, float arg2, float arg3, float arg4 )
      {
         _original( __instance, value, arg2, arg3, arg4 );

         Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_SetCharArray_Hook1
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TMP_Text.ClrType, "SetCharArray", new[] { typeof( char[] ) } );
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static Action<object, char[]> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, char[]>>();
      }

      static void MM_Detour( object __instance, char[] value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_SetCharArray_Hook2
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TMP_Text.ClrType, "SetCharArray", new[] { typeof( char[] ), typeof( int ), typeof( int ) } );
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static Action<object, char[], int, int> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, char[], int, int>>();
      }

      static void MM_Detour( object __instance, char[] value, int arg2, int arg3 )
      {
         _original( __instance, value, arg2, arg3 );

         Postfix( __instance );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_SetCharArray_Hook3
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TMP_Text.ClrType, "SetCharArray", new[] { typeof( int[] ), typeof( int ), typeof( int ) } );
      }

      static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static Action<object, int[], int, int> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, int[], int, int>>();
      }

      static void MM_Detour( object __instance, int[] value, int arg2, int arg3 )
      {
         _original( __instance, value, arg2, arg3 );

         Postfix( __instance );
      }
   }
}

#endif
