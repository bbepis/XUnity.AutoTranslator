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

         typeof( TMP_Text_SetText_Hook1 ),
         typeof( TMP_Text_SetText_Hook2 ),
         typeof( TMP_Text_SetText_Hook3 ),

         typeof( TMP_Text_SetCharArray_Hook1 ),
         typeof( TMP_Text_SetCharArray_Hook2 ),
         typeof( TMP_Text_SetCharArray_Hook3 ),

#if MANAGED
         typeof( TextWindow_SetText_Hook ),
         typeof( TeshMeshProUGUI_text_Hook ),
         typeof( TeshMeshPro_text_Hook ),
#endif
      };

      public static readonly Type[] DisableScrollInTmp = new[] {
         typeof( TMP_Text_maxVisibleCharacters_Hook ),
      };
   }

#if MANAGED
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
#endif

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_SetText_Hook1
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetText", new[] { typeof( StringBuilder ) } );
#else
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetText", new[] { typeof( Il2CppSystem.Text.StringBuilder ) } );
#endif
      }

      static void Postfix( Component __instance )
      {
#if IL2CPP
         __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TMP_Text.ClrType );
#endif

         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      static Action<Component, StringBuilder> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, StringBuilder>>();
      }

      static void MM_Detour( Component __instance, StringBuilder value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
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
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetText", new[] { typeof( string ), typeof( bool ) } );
      }

      static void Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      static Action<Component, string, bool> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, string, bool>>();
      }

      static void MM_Detour( Component __instance, string value, bool arg2 )
      {
         _original( __instance, value, arg2 );

         Postfix( __instance );
      }
#endif
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class TMP_Text_SetText_Hook3
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.TMP_Text != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.TMP_Text.ClrType, "SetText", new[] { typeof( string ), typeof( float ), typeof( float ), typeof( float ) } );
      }

      static void Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      delegate void OriginalMethod( Component arg1, string arg2, float arg3, float arg4, float arg5 );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static void MM_Detour( Component __instance, string value, float arg2, float arg3, float arg4 )
      {
         _original( __instance, value, arg2, arg3, arg4 );

         Postfix( __instance );
      }
#endif
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
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetCharArray", new[] { typeof( char[] ) } );
#else
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetCharArray", new[] { typeof( Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<char> ) } );
#endif
      }

      static void Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      static Action<Component, char[]> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, char[]>>();
      }

      static void MM_Detour( Component __instance, char[] value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
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
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetCharArray", new[] { typeof( char[] ), typeof( int ), typeof( int ) } );
#else
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetCharArray", new[] { typeof( Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<char> ), typeof( int ), typeof( int ) } );
#endif
      }

      static void Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      static Action<Component, char[], int, int> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, char[], int, int>>();
      }

      static void MM_Detour( Component __instance, char[] value, int arg2, int arg3 )
      {
         _original( __instance, value, arg2, arg3 );

         Postfix( __instance );
      }
#endif
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
#if MANAGED
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetCharArray", new[] { typeof( int[] ), typeof( int ), typeof( int ) } );
#else
         return AccessToolsShim.Method( UnityTypes.TMP_Text?.ClrType, "SetCharArray", new[] { typeof( Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int> ), typeof( int ), typeof( int ) } );
#endif
      }

      static void Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

#if MANAGED
      static Action<Component, int[], int, int> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, int[], int, int>>();
      }

      static void MM_Detour( Component __instance, int[] value, int arg2, int arg3 )
      {
         _original( __instance, value, arg2, arg3 );

         Postfix( __instance );
      }
#endif
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
         return AccessToolsShim.Method( UnityTypes.TextMeshProUGUI?.ClrType, "OnEnable" );
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
      }

      static void Postfix( Component __instance )
      {
#if IL2CPP
         __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextMeshProUGUI.ClrType );
#endif

         _Postfix( __instance );
      }

#if IL2CPP
      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.TextMeshProUGUI_Methods.IL2CPP.OnEnable;
      }

      static void ML_Detour( IntPtr instance )
      {
         Il2CppUtilities.InvokeMethod( UnityTypes.TextMeshProUGUI_Methods.IL2CPP.OnEnable, instance );

         var __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( instance, UnityTypes.TextMeshProUGUI.ClrType );
         _Postfix( __instance );
      }
#endif

#if MANAGED
      static Action<Component> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component>>();
      }

      static void MM_Detour( Component __instance )
      {
         _original( __instance );

         Postfix( __instance );
      }
#endif
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
         return AccessToolsShim.Method( UnityTypes.TextMeshPro?.ClrType, "OnEnable" );
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, true );
      }

      static void Postfix( Component __instance )
      {
#if IL2CPP
         __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( __instance.Pointer, UnityTypes.TextMeshPro.ClrType );
#endif

         _Postfix( __instance );
      }

#if IL2CPP
      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.TextMeshPro_Methods.IL2CPP.OnEnable;
      }

      static void ML_Detour( IntPtr instance )
      {
         Il2CppUtilities.InvokeMethod( UnityTypes.TextMeshPro_Methods.IL2CPP.OnEnable, instance );

         var __instance = (Component)Il2CppUtilities.CreateProxyComponentWithDerivedType( instance, UnityTypes.TextMeshPro.ClrType );
         _Postfix( __instance );
      }
#endif

#if MANAGED
      static Action<Component> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component>>();
      }

      static void MM_Detour( Component __instance )
      {
         _original( __instance );

         Postfix( __instance );
      }
#endif
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
         return AccessToolsShim.Property( UnityTypes.TMP_Text?.ClrType, "text" )?.GetSetMethod();
      }

      static void _Postfix( Component __instance )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
      }

      static void Postfix( Component __instance )
      {
#if IL2CPP
         __instance = __instance.CreateTextMeshProDerivedProxy();
#endif

         _Postfix( __instance );
      }

#if IL2CPP
      static IntPtr TargetMethodPointer()
      {
         return UnityTypes.TMP_Text_Methods.IL2CPP.set_text;
      }

      static void ML_Detour( IntPtr instance, IntPtr value )
      {
         Il2CppUtilities.InvokeMethod( UnityTypes.TMP_Text_Methods.IL2CPP.set_text, instance, value );

         var __instance = instance.CreateTextMeshProDerivedProxy();
         _Postfix( __instance );
      }
#endif

#if MANAGED
      static Action<Component, string> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, string>>();
      }

      static void MM_Detour( Component __instance, string value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
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
         return AccessToolsShim.Property( UnityTypes.TMP_Text?.ClrType, "maxVisibleCharacters" )?.GetSetMethod();
      }

      static void Prefix( Component __instance, ref int value )
      {
         var info = __instance.GetTextTranslationInfo();
         if( info != null && info.IsTranslated && 0 < value )
         {
            value = 99999;
         }
      }

#if MANAGED
      static Action<Component, int> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, int>>();
      }

      static void MM_Detour( Component __instance, int value )
      {
         Prefix( __instance, ref value );

         _original( __instance, value );
      }
#endif
   }
}
