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
using XUnity.Common.Extensions;
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
         return AccessToolsShim.Method( UnityTypes.DicingTextures?.ClrType, "GetTexture", new[] { typeof( string ) } );
      }

      public static void Postfix( object __instance, ref Texture2D __result )
      {
         AutoTranslationPlugin.Current.Hook_ImageChanged( ref __result, false );
      }

#if MANAGED
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
#endif
   }

   internal static class Sprite_texture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Sprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Sprite?.ClrType, "texture" )?.GetGetMethod();
      }

      static void Postfix( ref Texture2D __result )
      {
         AutoTranslationPlugin.Current.Hook_ImageChanged( ref __result, true );
      }

#if MANAGED
      static Func<Sprite, Texture2D> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Func<Sprite, Texture2D>>();
      }

      static Texture2D MM_Detour( Sprite __instance )
      {
         var result = _original( __instance );

         Postfix( ref result );

         return result;
      }
#endif
   }

   internal static class SpriteRenderer_sprite_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.SpriteRenderer != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.SpriteRenderer?.ClrType, "sprite" )?.GetSetMethod();
      }

      public static void Prefix( SpriteRenderer __instance, ref Sprite value )
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

#if MANAGED
      static Action<SpriteRenderer, Sprite> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<SpriteRenderer, Sprite>>();
      }

      static void MM_Detour( SpriteRenderer __instance, Sprite sprite )
      {
         //var prev = sprite;
         Prefix( __instance, ref sprite );

         _original( __instance, sprite );

         //if( prev != sprite )
         //{
         //   Postfix( __instance, ref sprite );
         //}
      }
#endif
   }

   internal static class CubismRenderer_MainTexture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.CubismRenderer != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.CubismRenderer.ClrType, "MainTexture" )?.GetSetMethod();
      }

      public static void Prefix( Component __instance, ref Texture2D value )
      {
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref value, true, false );
      }

#if MANAGED
      static Action<Component, Texture2D> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, Texture2D>>();
      }

      static void MM_Detour( Component __instance, ref Texture2D value )
      {
         Prefix( __instance, ref value );

         _original( __instance, value );
      }
#endif
   }

   internal static class CubismRenderer_TryInitialize_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.CubismRenderer != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.CubismRenderer.ClrType, "TryInitialize" );
      }

      public static void Prefix( Component __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, true, true );
      }

#if MANAGED
      static Action<Component> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component>>();
      }

      static void MM_Detour( Component __instance )
      {
         Prefix( __instance );

         _original( __instance );
      }
#endif
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

      public static void Prefix( Material __instance, ref Texture value )
      {
         if( value.TryCastTo<Texture2D>( out var texture2d ) )
         {
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref texture2d, true, false );
            value = texture2d;
         }
      }

#if MANAGED
      static Action<Material, Texture> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Material, Texture>>();
      }

      static void MM_Detour( Material __instance, ref Texture value )
      {
         Prefix( __instance, ref value );

         _original( __instance, value );
      }
#endif
   }

   internal static class MaskableGraphic_OnEnable_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.MaskableGraphic != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.MaskableGraphic?.ClrType, "OnEnable" );
      }

      public static void Postfix( Component __instance )
      {
         var type = __instance.GetUnityType();
         if( ( UnityTypes.Image != null && UnityTypes.Image.IsAssignableFrom( type ) )
            || ( UnityTypes.RawImage != null && UnityTypes.RawImage.IsAssignableFrom( type ) ) )
         {
            Texture2D _ = null;
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, true );
         }
      }

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

   // FIXME: Cannot 'set' texture of sprite. MUST enable hook sprite
   internal static class Image_sprite_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Image != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Image?.ClrType, "sprite" )?.GetSetMethod();
      }

      public static void Postfix( Component __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
      static Action<Component, Sprite> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, Sprite>>();
      }

      static void MM_Detour( Component __instance, Sprite value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
   }

   internal static class Image_overrideSprite_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Image != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Image?.ClrType, "overrideSprite" )?.GetSetMethod();
      }

      public static void Postfix( Component __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
      static Action<Component, Sprite> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, Sprite>>();
      }

      static void MM_Detour( Component __instance, Sprite value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
   }

   internal static class Image_material_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.Image != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.Image?.ClrType, "material" )?.GetSetMethod();
      }

      public static void Postfix( Component __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
      static Action<Component, Material> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, Material>>();
      }

      static void MM_Detour( Component __instance, Material value )
      {
         _original( __instance, value );

         Postfix( __instance );
      }
#endif
   }

   internal static class RawImage_texture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.RawImage != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.RawImage?.ClrType, "texture" )?.GetSetMethod();
      }

      public static void Prefix( Component __instance, ref Texture value )
      {
         if( value.TryCastTo<Texture2D>( out var texture2d ) )
         {
            AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref texture2d, true, false );
            value = texture2d;
         }
      }

#if MANAGED
      static Action<Component, Texture> _original;

      static void MM_Init( object detour )
      {
         _original = detour.GenerateTrampolineEx<Action<Component, Texture>>();
      }

      static void MM_Detour( Component __instance, Texture value )
      {
         Prefix( __instance, ref value );

         _original( __instance, value );
      }
#endif
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

#if MANAGED
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
#endif
   }

   internal static class UIAtlas_spriteMaterial_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UIAtlas != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UIAtlas?.ClrType, "spriteMaterial" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
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
#endif
   }

   internal static class UISprite_OnInit_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UISprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.UISprite?.ClrType, "OnInit" );
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, true );
      }

#if MANAGED
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
#endif
   }

   internal static class UISprite_material_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UISprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UISprite?.ClrType, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
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
#endif
   }

   internal static class UISprite_atlas_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UISprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UISprite?.ClrType, "atlas" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
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
#endif
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
         return AccessToolsShim.Property( UnityTypes.UITexture?.ClrType, "mainTexture" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
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
#endif
   }

   internal static class UITexture_material_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UITexture != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UITexture?.ClrType, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
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
#endif
   }

   internal static class UIRect_OnInit_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UIRect != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Method( UnityTypes.UIRect?.ClrType, "OnInit" );
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, true );
      }

#if MANAGED
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
#endif
   }

   internal static class UI2DSprite_sprite2D_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UI2DSprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UI2DSprite?.ClrType, "sprite2D" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
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
#endif
   }

   internal static class UI2DSprite_material_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UI2DSprite != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UI2DSprite?.ClrType, "material" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
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
#endif
   }

   internal static class UIPanel_clipTexture_Hook
   {
      static bool Prepare( object instance )
      {
         return UnityTypes.UIPanel != null;
      }

      static MethodBase TargetMethod( object instance )
      {
         return AccessToolsShim.Property( UnityTypes.UIPanel.ClrType, "clipTexture" )?.GetSetMethod();
      }

      public static void Postfix( object __instance )
      {
         Texture2D _ = null;
         AutoTranslationPlugin.Current.Hook_ImageChangedOnComponent( __instance, ref _, false, false );
      }

#if MANAGED
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
#endif
   }
}
