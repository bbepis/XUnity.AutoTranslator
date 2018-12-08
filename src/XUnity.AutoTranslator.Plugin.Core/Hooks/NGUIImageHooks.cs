using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   public static class NGUIImageHooks
   {
      public static readonly Type[] All = new Type[] {
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
         //typeof( UIFont_dynamicFont_Hook ),
         //typeof( UIFont_material_Hook ),
         //typeof( UILabel_bitmapFont_Hook ),
         //typeof( UILabel_trueTypeFont_Hook ),
      };
   }

   [Harmony]
   public static class UIAtlas_spriteMaterial_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIAtlas != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UIAtlas, "spriteMaterial" ).GetSetMethod();
      }

      public static void Prefix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, true );
      }
   }
   
   [Harmony]
   public static class UISprite_OnInit_Hook
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
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class UISprite_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UISprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UISprite, "material" ).GetSetMethod();
      }

      public static void Prefix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, true );
      }
   }

   [Harmony]
   public static class UISprite_atlas_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UISprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UISprite, "atlas" ).GetSetMethod();
      }

      public static void Prefix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, true );
      }
   }
   
   [Harmony]
   public static class UITexture_mainTexture_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UITexture != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UITexture, "mainTexture" ).GetSetMethod();
      }

      public static void Prefix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, true );
      }
   }

   [Harmony]
   public static class UITexture_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UITexture != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UITexture, "material" ).GetSetMethod();
      }

      public static void Prefix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, true );
      }
   }

   [Harmony]
   public static class UIRect_OnInit_Hook
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
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class UI2DSprite_sprite2D_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UI2DSprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UI2DSprite, "sprite2D" ).GetSetMethod();
      }

      public static void Prefix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, true );
      }
   }

   [Harmony]
   public static class UI2DSprite_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UI2DSprite != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UI2DSprite, "material" ).GetSetMethod();
      }

      public static void Prefix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, null, true );
      }
   }


   [Harmony]
   public static class UIFont_material_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIFont != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UIFont, "material" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class UIFont_dynamicFont_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIFont != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UIFont, "dynamicFont" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class UIPanel_clipTexture_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UIPanel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UIPanel, "clipTexture" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class UILabel_bitmapFont_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UILabel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UILabel, "bitmapFont" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }

   [Harmony]
   public static class UILabel_trueTypeFont_Hook
   {
      static bool Prepare( HarmonyInstance instance )
      {
         return ClrTypes.UILabel != null;
      }

      static MethodBase TargetMethod( HarmonyInstance instance )
      {
         return AccessTools.Property( ClrTypes.UILabel, "trueTypeFont" ).GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance );
      }
   }
}
