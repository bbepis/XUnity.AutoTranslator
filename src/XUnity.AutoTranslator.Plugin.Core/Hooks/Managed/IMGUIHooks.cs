#if MANAGED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.MonoMod;
using XUnity.Common.Utilities;
using static UnityEngine.GUI;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class IMGUIHooks
   {
      public static readonly Type[][] All = new[] {
         new[] { typeof( GUI_BeginGroup_Hook_New ), typeof( GUI_BeginGroup_Hook ) },
         new[] { typeof( GUI_DoLabel_Hook_New ), typeof( GUI_DoLabel_Hook ) },
         new[] { typeof( GUI_DoButton_Hook_New ), typeof( GUI_DoButton_Hook ) },
         new[] { typeof( GUI_DoButtonGrid_Hook_2019 ), typeof( GUI_DoButtonGrid_Hook_2018 ), typeof( GUI_DoButtonGrid_Hook ) },
         new[] { typeof( GUI_DoToggle_Hook_New ), typeof( GUI_DoToggle_Hook ) },
         new[] { typeof( GUI_Box_Hook ) },
         new[] { typeof( GUI_DoRepeatButton_Hook ) },
         new[] { typeof( GUI_DoModalWindow_Hook ) },
         new[] { typeof( GUI_DoWindow_Hook ) },
      };
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_BeginGroup_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "BeginGroup", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      static Action<Rect, GUIContent, GUIStyle> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Rect, GUIContent, GUIStyle>>();
      }

      static void MM_Detour( Rect arg1, GUIContent arg2, GUIStyle arg3 )
      {
         Prefix( arg2 );

         _original( arg1, arg2, arg3 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_BeginGroup_Hook_New
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "BeginGroup", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ), typeof( Vector2 ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      static Action<Rect, GUIContent, GUIStyle, Vector2> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Rect, GUIContent, GUIStyle, Vector2>>();
      }

      static void MM_Detour( Rect arg1, GUIContent arg2, GUIStyle arg3, Vector2 arg4 )
      {
         Prefix( arg2 );

         _original( arg1, arg2, arg3, arg4 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_Box_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "Box", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      static Action<Rect, GUIContent, GUIStyle> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Rect, GUIContent, GUIStyle>>();
      }

      static void MM_Detour( Rect arg1, GUIContent arg2, GUIStyle arg3 )
      {
         Prefix( arg2 );

         _original( arg1, arg2, arg3 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoRepeatButton_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoRepeatButton", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ), typeof( FocusType ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      static Func<Rect, GUIContent, GUIStyle, FocusType, bool> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<Rect, GUIContent, GUIStyle, FocusType, bool>>();
      }

      static bool MM_Detour( Rect arg1, GUIContent arg2, GUIStyle arg3, FocusType arg4 )
      {
         Prefix( arg2 );

         return _original( arg1, arg2, arg3, arg4 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoLabel_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoLabel", new[] { typeof( Rect ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      static Action<Rect, GUIContent, IntPtr> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Rect, GUIContent, IntPtr>>();
      }

      static void MM_Detour( Rect arg1, GUIContent arg2, IntPtr arg3 )
      {
         Prefix( arg2 );

         _original( arg1, arg2, arg3 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoLabel_Hook_New
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoLabel", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      static Action<Rect, GUIContent, GUIStyle> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Rect, GUIContent, GUIStyle>>();
      }

      static void MM_Detour( Rect arg1, GUIContent arg2, GUIStyle arg3 )
      {
         Prefix( arg2 );

         _original( arg1, arg2, arg3 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoButton_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoButton", new[] { typeof( Rect ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      static Func<Rect, GUIContent, IntPtr, bool> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<Rect, GUIContent, IntPtr, bool>>();
      }

      static bool MM_Detour( Rect arg1, GUIContent arg2, IntPtr arg3 )
      {
         Prefix( arg2 );

         return _original( arg1, arg2, arg3 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoButton_Hook_New
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoButton", new[] { typeof( Rect ), typeof( int ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      static Func<Rect, int, GUIContent, GUIStyle, bool> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<Rect, int, GUIContent, GUIStyle, bool>>();
      }

      static bool MM_Detour( Rect arg1, int arg2, GUIContent arg3, GUIStyle arg4 )
      {
         Prefix( arg3 );

         return _original( arg1, arg2, arg3, arg4 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoModalWindow_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoModalWindow", new[] { typeof( int ), typeof( Rect ), typeof( WindowFunction ), typeof( GUIContent ), typeof( GUIStyle ), typeof( GUISkin ) } );
      }

      static void Prefix( int id, WindowFunction func, GUIContent content )
      {
         IMGUIBlocker.BlockIfConfigured( func, id );
         IMGUIPluginTranslationHooks.HookIfConfigured( func );

         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      delegate Rect OriginalMethod( int arg1, Rect arg2, WindowFunction arg3, GUIContent arg4, GUIStyle arg5, GUISkin arg6 );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static Rect MM_Detour( int arg1, Rect arg2, WindowFunction arg3, GUIContent arg4, GUIStyle arg5, GUISkin arg6 )
      {
         Prefix( arg1, arg3, arg4 );

         return _original( arg1, arg2, arg3, arg4, arg5, arg6 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoWindow_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoWindow", new[] { typeof( int ), typeof( Rect ), typeof( WindowFunction ), typeof( GUIContent ), typeof( GUIStyle ), typeof( GUISkin ), typeof( bool ) } );
      }

      static void Prefix( int id, WindowFunction func, GUIContent title )
      {
         IMGUIBlocker.BlockIfConfigured( func, id );
         IMGUIPluginTranslationHooks.HookIfConfigured( func );

         AutoTranslationPlugin.Current.Hook_TextChanged( title, false );
      }

      delegate Rect OriginalMethod( int arg1, Rect arg2, WindowFunction arg3, GUIContent arg4, GUIStyle arg5, GUISkin arg6, bool arg7 );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static Rect MM_Detour( int arg1, Rect arg2, WindowFunction arg3, GUIContent arg4, GUIStyle arg5, GUISkin arg6, bool arg7 )
      {
         Prefix( arg1, arg3, arg4 );

         return _original( arg1, arg2, arg3, arg4, arg5, arg6, arg7 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoButtonGrid_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoButtonGrid", new[] { typeof( Rect ), typeof( int ), typeof( GUIContent[] ), typeof( int ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent[] contents )
      {
         foreach( var content in contents )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }

      delegate int OriginalMethod( Rect arg1, int arg2, GUIContent[] arg3, int arg4, GUIStyle arg5, GUIStyle arg6, GUIStyle arg7, GUIStyle arg8 );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static int MM_Detour( Rect arg1, int arg2, GUIContent[] arg3, int arg4, GUIStyle arg5, GUIStyle arg6, GUIStyle arg7, GUIStyle arg8 )
      {
         Prefix( arg3 );

         return _original( arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoButtonGrid_Hook_2018
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null && UnityTypes.GUI_ToolbarButtonSize != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoButtonGrid", new[] { typeof( Rect ), typeof( int ), typeof( GUIContent[] ), typeof( string[] ), typeof( int ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ), UnityTypes.GUI_ToolbarButtonSize.ClrType } );
      }

      static void Prefix( GUIContent[] contents )
      {
         foreach( var content in contents )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }

      delegate int OriginalMethod( Rect arg1, int arg2, GUIContent[] arg3, string[] arg4, int arg5, GUIStyle arg6, GUIStyle arg7, GUIStyle arg8, GUIStyle arg9, int arg10 );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static int MM_Detour( Rect arg1, int arg2, GUIContent[] arg3, string[] arg4, int arg5, GUIStyle arg6, GUIStyle arg7, GUIStyle arg8, GUIStyle arg9, int arg10 )
      {
         Prefix( arg3 );

         return _original( arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoButtonGrid_Hook_2019
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null && UnityTypes.GUI_ToolbarButtonSize != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoButtonGrid", new[] { typeof( Rect ), typeof( int ), typeof( GUIContent[] ), typeof( string[] ), typeof( int ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ), UnityTypes.GUI_ToolbarButtonSize.ClrType, typeof( bool[] ) } );
      }

      static void Prefix( GUIContent[] contents )
      {
         foreach( var content in contents )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }

      delegate int OriginalMethod( Rect arg1, int arg2, GUIContent[] arg3, string[] arg4, int arg5, GUIStyle arg6, GUIStyle arg7, GUIStyle arg8, GUIStyle arg9, int arg10, bool[] arg11 );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static int MM_Detour( Rect arg1, int arg2, GUIContent[] arg3, string[] arg4, int arg5, GUIStyle arg6, GUIStyle arg7, GUIStyle arg8, GUIStyle arg9, int arg10, bool[] arg11 )
      {
         Prefix( arg3 );

         return _original( arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoToggle_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoToggle", new[] { typeof( Rect ), typeof( int ), typeof( bool ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      delegate bool OriginalMethod( Rect arg1, int arg2, bool arg3, GUIContent arg4, IntPtr arg5 );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static bool MM_Detour( Rect arg1, int arg2, bool arg3, GUIContent arg4, IntPtr arg5 )
      {
         Prefix( arg4 );

         return _original( arg1, arg2, arg3, arg4, arg5 );
      }
   }

   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_DoToggle_Hook_New
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.GUI.ClrType, "DoToggle", new[] { typeof( Rect ), typeof( int ), typeof( bool ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
      }

      delegate bool OriginalMethod( Rect arg1, int arg2, bool arg3, GUIContent arg4, GUIStyle arg5 );

      static OriginalMethod _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<OriginalMethod>();
      }

      static bool MM_Detour( Rect arg1, int arg2, bool arg3, GUIContent arg4, GUIStyle arg5 )
      {
         Prefix( arg4 );

         return _original( arg1, arg2, arg3, arg4, arg5 );
      }
   }
}

#endif
