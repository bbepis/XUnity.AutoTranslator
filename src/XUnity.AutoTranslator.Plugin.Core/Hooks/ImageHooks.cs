using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class ImageHooks
   {
      public static readonly Type[] All = new[] {
         typeof( MaskableGraphic_OnEnable_Hook ),
         typeof( Image_sprite_Hook ),
         typeof( Image_overrideSprite_Hook ),
         typeof( Image_material_Hook ),
         typeof( RawImage_texture_Hook ),
         typeof( Cursor_SetCursor_Hook ),
         typeof( SpriteRenderer_sprite_Hook ),

         // fallback hooks on material (Prefix hooks)
         typeof( Material_mainTexture_Hook ),

         // Live2D
         typeof( CubismRenderer_MainTexture_Hook ),
         typeof( CubismRenderer_TryInitialize_Hook ),

         // NGUI
         typeof( UIAtlas_spriteMaterial_Hook ),
         typeof( UISprite_OnInit_Hook ),
         typeof( UISprite_material_Hook ),
         typeof( UISprite_atlas_Hook ),
         typeof( UI2DSprite_sprite2D_Hook ),
         typeof( UI2DSprite_material_Hook ),
         typeof( UITexture_mainTexture_Hook ),
         typeof( UITexture_material_Hook ),
         typeof( UIPanel_clipTexture_Hook ),
         typeof( UIRect_OnInit_Hook ),
      };
   }

   [Harmony]
   internal static class SpriteRenderer_sprite_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.SpriteRenderer != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.SpriteRenderer, "sprite" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class CubismRenderer_MainTexture_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.CubismRenderer != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.CubismRenderer, "MainTexture" )?.GetSetMethod();
      }

      public static void Prefix( object __instance, Texture2D value )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, value, true , false);
      }
   }

   [Harmony]
   internal static class CubismRenderer_TryInitialize_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.CubismRenderer != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.CubismRenderer, "TryInitialize" );
      }

      public static void Prefix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, true, true );
      }
   }

   [Harmony]
   internal static class Material_mainTexture_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( typeof( Material ), "mainTexture" )?.GetSetMethod();
      }

      public static void Prefix( object __instance, Texture value )
      {
         if( value is Texture2D texture2d )
         {
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, texture2d, true, false );
         }
      }
   }

   [Harmony]
   internal static class MaskableGraphic_OnEnable_Hook
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
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, true );
         }
      }
   }

   [Harmony]
   internal static class Image_sprite_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( typeof( Image ), "sprite" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class Image_overrideSprite_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( typeof( Image ), "overrideSprite" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class Image_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( typeof( Image ), "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class RawImage_texture_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( typeof( RawImage ), "texture" )?.GetSetMethod();
      }

      public static void Prefix( object __instance, Texture value )
      {
         if( value is Texture2D texture2d )
         {
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, texture2d, true, false );
         }
      }
   }

   [Harmony]
   internal static class Cursor_SetCursor_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return true;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( typeof( Cursor ), "SetCursor", new[] { typeof( Texture2D ), typeof( Vector2 ), typeof( CursorMode ) } );
      }

      public static void Prefix( Texture2D texture )
      {
         AutoTranslationPlugin.Current.Hook_ImageChanged( texture, true );
      }
   }

   [Harmony]
   internal static class UIAtlas_spriteMaterial_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIAtlas != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UIAtlas, "spriteMaterial" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UISprite_OnInit_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UISprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( Constants.ClrTypes.UISprite, "OnInit" );
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, true );
      }
   }

   [Harmony]
   internal static class UISprite_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UISprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UISprite, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UISprite_atlas_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UISprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UISprite, "atlas" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UITexture_mainTexture_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UITexture != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UITexture, "mainTexture" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UITexture_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UITexture != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UITexture, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UIRect_OnInit_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIRect != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Method( ClrTypes.UIRect, "OnInit" );
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, true );
      }
   }

   [Harmony]
   internal static class UI2DSprite_sprite2D_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UI2DSprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UI2DSprite, "sprite2D" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UI2DSprite_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UI2DSprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UI2DSprite, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UIPanel_clipTexture_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIPanel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UIPanel, "clipTexture" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }


   [Harmony]
   internal static class UIFont_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIFont != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UIFont, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UIFont_dynamicFont_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIFont != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UIFont, "dynamicFont" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UILabel_bitmapFont_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UILabel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UILabel, "bitmapFont" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }

   [Harmony]
   internal static class UILabel_trueTypeFont_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UILabel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UILabel, "trueTypeFont" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, false, false );
      }
   }
}
