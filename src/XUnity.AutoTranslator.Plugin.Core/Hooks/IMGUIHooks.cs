using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using UnityEngine;
using static UnityEngine.GUI;

namespace XUnity.AutoTranslator.Plugin.Core.IMGUI
{
   public static class IMGUIHooks
   {
      public static bool HooksOverriden = false;

      public static readonly Type[] All = new[] {
         typeof( BeginGroupHook ),
         typeof( BoxHook ),
         typeof( DoRepeatButtonHook ),
         typeof( DoLabelHook ),
         typeof( DoButtonHook ),
         typeof( DoModalWindowHook ),
         typeof( DoWindowHook ),
         typeof( DoTextFieldHook ),
         typeof( DoToggleHook ),
      };
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class BeginGroupHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "BeginGroup", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class BoxHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "Box", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }

   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class DoRepeatButtonHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "DoRepeatButton", new[] { typeof( Rect ), typeof( GUIContent ), typeof( GUIStyle ), typeof( FocusType ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class DoLabelHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "DoLabel", new[] { typeof( Rect ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class DoButtonHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "DoButton", new[] { typeof( Rect ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class DoModalWindowHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "DoModalWindow", new[] { typeof( int ), typeof( Rect ), typeof( WindowFunction ), typeof( GUIContent ), typeof( GUIStyle ), typeof( GUISkin ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class DoWindowHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "DoWindow", new[] { typeof( int ), typeof( Rect ), typeof( WindowFunction ), typeof( GUIContent ), typeof( GUIStyle ), typeof( GUISkin ), typeof( bool ) } );
      }

      static void Prefix( GUIContent title )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( title, false );
         }
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class DoButtonGridHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "DoButtonGrid", new[] { typeof( Rect ), typeof( int ), typeof( GUIContent[] ), typeof( int ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ), typeof( GUIStyle ) } );
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
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class DoTextFieldHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "DoTextField", new[] { typeof( Rect ), typeof( int ), typeof( GUIContent ), typeof( bool ), typeof( int ), typeof( GUIStyle ), typeof( string ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }
   }

   [Harmony, HarmonyAfter( Constants.KnownPlugins.DynamicTranslationLoader )]
   public static class DoToggleHook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return Constants.ClrTypes.GUI != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.GUI, "DoToggle", new[] { typeof( Rect ), typeof( int ), typeof( bool ), typeof( GUIContent ), typeof( IntPtr ) } );
      }

      static void Prefix( GUIContent content )
      {
         if( !IMGUIHooks.HooksOverriden )
         {
            AutoTranslationPlugin.Current.Hook_TextChanged( content, false );
         }
      }
   }
}
