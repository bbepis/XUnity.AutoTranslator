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

namespace XUnity.AutoTranslator.Plugin.Core.Hooks.IMGUI
{
   internal static class IMGUIHooks
   {
      public static bool HooksOverriden = false;

      public static readonly Type[] All = new[] {
         //typeof( GUIContent_text_Hook ),
         //typeof( GUIContent_Temp_Hook1 ),
         //typeof( GUIContent_Temp_Hook2 ),
         //typeof( GUIContent_Temp_Hook3 ),

         typeof( GUI_BeginGroup_Hook ),
         typeof( GUI_Box_Hook ),
         typeof( GUI_DoRepeatButton_Hook ),
         typeof( GUI_DoLabel_Hook ),
         typeof( GUI_DoButton_Hook ),
         typeof( GUI_DoModalWindow_Hook ),
         typeof( GUI_DoWindow_Hook ),
         typeof( GUI_DoButtonGrid_Hook ),
         typeof( GUI_DoToggle_Hook ),
      };
   }


   //[HarmonyPriority( HookPriority.Last )]
   //internal static class GUIContent_text_Hook
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return ClrTypes.GUIContent != null;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessTools.Property( ClrTypes.GUIContent, "text" )?.GetSetMethod();
   //   }

   //   static void Postfix( GUIContent __instance )
   //   {
   //      if( !IMGUIHooks.HooksOverriden )
   //      {
   //         AutoTranslationPlugin.Current.Hook_TextChanged( __instance, false );
   //      }
   //   }
   //}

   //[HarmonyPriority( HookPriority.Last )]
   //internal static class GUIContent_Temp_Hook1
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return ClrTypes.GUIContent != null;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessTools.Method( ClrTypes.GUIContent, "Temp", new[] { typeof( string ) } );
   //   }

   //   static void Postfix( GUIContent __result )
   //   {
   //      if( !IMGUIHooks.HooksOverriden )
   //      {
   //         AutoTranslationPlugin.Current.Hook_TextChanged( __result, false );
   //      }
   //   }
   //}

   //[HarmonyPriority( HookPriority.Last )]
   //internal static class GUIContent_Temp_Hook2
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return ClrTypes.GUIContent != null;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessTools.Method( ClrTypes.GUIContent, "Temp", new[] { typeof( string ), typeof( string ) } );
   //   }

   //   static void Postfix( GUIContent __result )
   //   {
   //      if( !IMGUIHooks.HooksOverriden )
   //      {
   //         AutoTranslationPlugin.Current.Hook_TextChanged( __result, false );
   //      }
   //   }
   //}

   //[HarmonyPriority( HookPriority.Last )]
   //internal static class GUIContent_Temp_Hook3
   //{
   //   static bool Prepare( object instance )
   //   {
   //      return ClrTypes.GUIContent != null;
   //   }

   //   static MethodBase TargetMethod( object instance )
   //   {
   //      return AccessTools.Method( ClrTypes.GUIContent, "Temp", new[] { typeof( string ), typeof( Texture ) } );
   //   }

   //   static void Postfix( GUIContent __result )
   //   {
   //      if( !IMGUIHooks.HooksOverriden )
   //      {
   //         AutoTranslationPlugin.Current.Hook_TextChanged( __result, false );
   //      }
   //   }
   //}












   [HookingHelperPriority( HookPriority.Last )]
   internal static class GUI_BeginGroup_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "BeginGroup", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
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
   internal static class GUI_Box_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "Box", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
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
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "DoRepeatButton", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ), typeof( FocusType ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
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
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "DoLabel", new[] { typeof( Rect ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
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
   internal static class GUI_DoButton_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "DoButton", new[] { typeof( Rect ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
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
   internal static class GUI_DoModalWindow_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "DoModalWindow", new[] { typeof( int ), typeof( Rect ), typeof( WindowFunction ), typeof( GUIContent ), typeof( GUIStyle ), typeof( GUISkin ) } );
      }

      static void Prefix( int id, WindowFunction func, GUIContent content )
      {
         if( Settings.BlacklistedIMGUIPlugins.Count > 0 )
         {
            IMGUIBlocker.BlockIfConfigured( func.Method, id );
         }

         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
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
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "DoWindow", new[] { typeof( int ), typeof( Rect ), typeof( WindowFunction ), typeof( GUIContent ), typeof( GUIStyle ), typeof( GUISkin ), typeof( bool ) } );
      }

      static void Prefix( int id, WindowFunction func, GUIContent title )
      {
         if( Settings.BlacklistedIMGUIPlugins.Count > 0 )
         {
            IMGUIBlocker.BlockIfConfigured( func.Method, id );
         }

         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( title, false );
         }
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
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "DoButtonGrid", new[] { typeof( Rect ), typeof( int ), typeof( GUIContent[] ), typeof( int ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent[] contents )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            foreach( var content in contents )
            {
               AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
            }
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
   internal static class GUI_DoToggle_Hook
   {
      static bool Prepare( object instance )
      {
         return ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( ClrTypes.GUI, "DoToggle", new[] { typeof( Rect ), typeof( int ), typeof( bool ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
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
}
