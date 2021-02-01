using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextureComponentExtensions
   {
      private static readonly string SetAllDirtyMethodName = "SetAllDirty";
      private static readonly string TexturePropertyName = "texture";
      private static readonly string MainTexturePropertyName = "mainTexture";
      private static readonly string CapitalMainTexturePropertyName = "MainTexture";
      private static readonly string MarkAsChangedMethodName = "MarkAsChanged";

      public static string GetPath( this object ui )
      {
         if( ui is Component comp )
         {
            var go = comp.gameObject;
            return go.GetPath();
         }
         return null;
      }

      public static Texture2D GetTexture( this object ui )
      {
         if( ui == null ) return null;

         if( ui is SpriteRenderer spriteRenderer )
         {
            return spriteRenderer.sprite?.texture;
         }
         else
         {
            // lets attempt some reflection for several known types
            var type = ui.GetType();
            var texture = type.CachedProperty( MainTexturePropertyName )?.Get( ui )
               ?? type.CachedProperty( TexturePropertyName )?.Get( ui )
               ?? type.CachedProperty( CapitalMainTexturePropertyName )?.Get( ui );

            return texture as Texture2D;
         }
      }

      public static Sprite SetTexture( this object ui, Texture2D texture, Sprite sprite, bool isPrefixHooked )
      {
         if( ui == null ) return null;

         var currentTexture = ui.GetTexture();

         if( currentTexture != texture )
         {
            if( Settings.EnableSpriteRendererHooking && ui is SpriteRenderer sr )
            {
               if( isPrefixHooked )
               {
                  return SafeCreateSprite( sr, sprite, texture );
               }
               else
               {
                  return SafeSetSprite( sr, sprite, texture );
               }
            }
            else
            {
               // This logic is only used in legacy mode and is not verified with NGUI
               var type = ui.GetType();
               type.CachedProperty( MainTexturePropertyName )?.Set( ui, texture );
               type.CachedProperty( TexturePropertyName )?.Set( ui, texture );
               type.CachedProperty( CapitalMainTexturePropertyName )?.Set( ui, texture );

               // special handling for UnityEngine.UI.Image
               var material = type.CachedProperty( "material" )?.Get( ui );
               if( material != null )
               {
                  var mainTextureProperty = material.GetType().CachedProperty( MainTexturePropertyName );
                  var materialTexture = mainTextureProperty?.Get( material );
                  if( ReferenceEquals( materialTexture, currentTexture ) )
                  {
                     mainTextureProperty?.Set( material, texture );
                  }
               }
            }
         }

         return null;
      }

      private static Sprite SafeSetSprite( SpriteRenderer sr, Sprite sprite, Texture2D texture )
      {
         var newSprite = Sprite.Create( texture, sprite != null ? sprite.rect : sr.sprite.rect, Vector2.zero );
         sr.sprite = newSprite;
         return newSprite;
      }

      private static Sprite SafeCreateSprite( SpriteRenderer sr, Sprite sprite, Texture2D texture )
      {
         var newSprite = Sprite.Create( texture, sprite != null ? sprite.rect : sr.sprite.rect, Vector2.zero );
         return newSprite;
      }

      public static void SetAllDirtyEx( this object ui )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( ClrTypes.Graphic != null && ClrTypes.Graphic.IsAssignableFrom( type ) )
         {
            ClrTypes.Graphic.CachedMethod( SetAllDirtyMethodName ).Invoke( ui );
         }
         else if( ui is not SpriteRenderer )
         {
            AccessToolsShim.Method( type, MarkAsChangedMethodName )?.Invoke( ui, null );
         }
      }
   }
}
