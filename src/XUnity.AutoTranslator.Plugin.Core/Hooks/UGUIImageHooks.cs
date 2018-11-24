using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using UnityEngine;
using UnityEngine.UI;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   public static class UGUIImageHooks
   {
      public static readonly Type[] All = new[] {
         typeof( MaskableGraphic_OnEnable_Hook ),
         typeof( Image_sprite_Hook ),
         typeof( Image_overrideSprite_Hook ),
         typeof( RawImage_texture_Hook ),
         typeof( Cursor_SetCursor_Hook )
      };
   }

   [Harmony]
   public static class MaskableGraphic_OnEnable_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( typeof( MaskableGraphic ), "OnEnable" );
      }

      public static void Postfix( object __instance )
      {
         if( __instance is Image || __instance is RawImage )
         {
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
         }
      }
   }

   [Harmony]
   public static class Image_sprite_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( typeof( Image ), "sprite" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class Image_overrideSprite_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( typeof( Image ), "overrideSprite" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class RawImage_texture_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( typeof( RawImage ), "texture" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class Cursor_SetCursor_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( typeof( Cursor ), "SetCursor", new[] { typeof( Texture2D ), typeof( Vector2 ), typeof( CursorMode ) } );
      }

      public static void Postfix( Texture2D texture )
      {
         AutoTranslationPlugin.Current.Hook_ImageChanged( texture );
      }
   }
}
