#if MANAGED

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Logging;
using XUnity.Common.MonoMod;

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

         // Utage
         typeof( DicingTextures_GetTexture_Hook ),
      };

      public static readonly Type[] Sprite = new[] {
         typeof( Sprite_texture_Hook )
      };

      public static readonly Type[] SpriteRenderer = new[] {
         typeof( SpriteRenderer_sprite_Hook ),
      };
   }

   internal static class DicingTextures_GetTexture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.DicingTextures != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.DicingTextures, "GetTexture", new[] { typeof( string ) } );
      }

      public static void Postfix( object __instance, ref Texture2D __result )
      {
         AutoTranslationPlugin.Current.Hook_ImageChanged( ref __result, false );
      }

      static Func<object, string, Texture2D> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<object, string, Texture2D>>();
      }

      static Texture2D MM_Detour( object __instance, string arg1 )
      {
         var result = _original( __instance, arg1 );

         Postfix( __instance, ref result );

         return result;
      }
   }

   internal static class Sprite_texture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Sprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Sprite, "texture" )?.GetGetMethod();
      }

      static void Postfix( ref Texture2D __result )
      {
         AutoTranslationPlugin.Current.Hook_ImageChanged( ref __result, true );
      }

      static Func<object, Texture2D> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<object, Texture2D>>();
      }

      static Texture2D MM_Detour( object __instance )
      {
         var result = _original( __instance );

         Postfix( ref result );

         return result;
      }
   }

   internal static class SpriteRenderer_sprite_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.SpriteRenderer != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.SpriteRenderer, "sprite" )?.GetSetMethod();
      }

      public static void Prefix( object __instance, ref Sprite value )
      {
         Texture2D texture;
         var prev = CallOrigin.ImageHooksEnabled;
         CallOrigin.ImageHooksEnabled = false;
         try
         {
            texture = value.texture;
         }
         finally
         {
            CallOrigin.ImageHooksEnabled = prev;
         }
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref value, ref texture, true, false );
      }

      //public static void Postfix( object __instance, ref Sprite value )
      //{
      //   Texture2D _ = null;
      //   AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      //}

      static Action<object, Sprite> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Sprite>>();
      }

      static void MM_Detour( object __instance, Sprite sprite )
      {
         //var prev = sprite;
         Prefix( __instance, ref sprite );

         _original( __instance, sprite );

         //if( prev != sprite )
         //{
         //   Postfix( __instance, ref sprite );
         //}
      }
   }

   internal static class CubismRenderer_MainTexture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.CubismRenderer != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.CubismRenderer, "MainTexture" )?.GetSetMethod();
      }

      public static void Prefix( object __instance, ref Texture2D value )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref value, true, false );
      }

      static Action<object, Texture2D> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Texture2D>>();
      }

      static void MM_Detour( object __instance, ref Texture2D value )
      {
         Prefix( __instance, ref value );

         _original( __instance, value );
      }
   }

   internal static class CubismRenderer_TryInitialize_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.CubismRenderer != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.CubismRenderer, "TryInitialize" );
      }

      public static void Prefix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, true, true );
      }

      static Action<object> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object>>();
      }

      static void MM_Detour( object __instance )
      {
         Prefix( __instance );

         _original( __instance );
      }
   }

   internal static class Material_mainTexture_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( typeof( Material ), "mainTexture" )?.GetSetMethod();
      }

      public static void Prefix( object __instance, ref Texture value )
      {
         if( value is Texture2D texture2d )
         {
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref texture2d, true, false );
            value = texture2d;
         }
      }

      static Action<object, Texture> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Texture>>();
      }

      static void MM_Detour( object __instance, ref Texture value )
      {
         Prefix( __instance, ref value );

         _original( __instance, value );
      }
   }

   internal static class MaskableGraphic_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.MaskableGraphic != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.MaskableGraphic, "OnEnable" );
      }

      public static void Postfix( object __instance )
      {
         var type = __instance.GetType();
         if( ( UnityTypes.Image != null && UnityTypes.Image.IsAssignableFrom( type ) )
            || ( UnityTypes.RawImage != null && UnityTypes.RawImage.IsAssignableFrom( type ) ) )
         {
            Texture2D _ = null;
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, true );
         }
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

   // FIXME: Cannot 'set' texture of sprite. MUST enable hook sprite
   internal static class Image_sprite_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Image != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Image, "sprite" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

      static Action<object, Sprite> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Sprite>>();
      }

      static void MM_Detour( object __instance, Sprite value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   internal static class Image_overrideSprite_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Image != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Image, "overrideSprite" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

      static Action<object, Sprite> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Sprite>>();
      }

      static void MM_Detour( object __instance, Sprite value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   internal static class Image_material_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Image != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Image, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

      static Action<object, Material> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Material>>();
      }

      static void MM_Detour( object __instance, Material value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   internal static class RawImage_texture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.RawImage != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.RawImage, "texture" )?.GetSetMethod();
      }

      public static void Prefix( object __instance, ref Texture value )
      {
         if( value is Texture2D texture2d )
         {
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref texture2d, true, false );
            value = texture2d;
         }
      }

      static Action<object, Texture> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Texture>>();
      }

      static void MM_Detour( object __instance, Texture value )
      {
         Prefix( __instance, ref value );

         _original( __instance, value );
      }
   }

   internal static class Cursor_SetCursor_Hook
   {
      static bool Prepare( object instance )
      {
         return true;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( typeof( Cursor ), "SetCursor", new[] { typeof( Texture2D ), typeof( Vector2 ), typeof( CursorMode ) } );
      }

      public static void Prefix( ref Texture2D texture )
      {
         AutoTranslationPlugin.Current.Hook_ImageChanged( ref texture, true );
      }

      static Action<Texture2D, Vector2, CursorMode> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Texture2D, Vector2, CursorMode>>();
      }

      static void MM_Detour( Texture2D texture, Vector2 arg2, CursorMode arg3 )
      {
         Prefix( ref texture );

         _original( texture, arg2, arg3 );
      }
   }

   internal static class UIAtlas_spriteMaterial_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UIAtlas != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UIAtlas, "spriteMaterial" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

      static Action<object, Material> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Material>>();
      }

      static void MM_Detour( object __instance, Material value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   internal static class UISprite_OnInit_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UISprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.UISprite, "OnInit" );
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, true );
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

   internal static class UISprite_material_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UISprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UISprite, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

      static Action<object, Material> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<object, Material>>();
      }

      static void MM_Detour( object __instance, Material value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
   }

   internal static class UISprite_atlas_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UISprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UISprite, "atlas" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
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

   // Could be postfix, but SETTER will be called later
   internal static class UITexture_mainTexture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UITexture != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UITexture, "mainTexture" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
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

   internal static class UITexture_material_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UITexture != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UITexture, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
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

   internal static class UIRect_OnInit_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UIRect != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.UIRect, "OnInit" );
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, true );
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

   internal static class UI2DSprite_sprite2D_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UI2DSprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UI2DSprite, "sprite2D" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
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

   internal static class UI2DSprite_material_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UI2DSprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UI2DSprite, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
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

   internal static class UIPanel_clipTexture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UIPanel != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UIPanel, "clipTexture" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
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
}

#endif
