using UnityEngine;
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

      public static void SetTexture( this object ui, Texture2D texture )
      {
         if( ui == null ) return;

         var currentTexture = ui.GetTexture();

         if( currentTexture != texture )
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

      public static void SetAllDirtyEx( this object ui )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( UnityTypes.Graphic != null && UnityTypes.Graphic.IsAssignableFrom( type ) )
         {
            UnityTypes.Graphic.CachedMethod( SetAllDirtyMethodName ).Invoke( ui );
         }
         else
         {
            AccessToolsShim.Method( type, MarkAsChangedMethodName )?.Invoke( ui, null );
         }
      }
   }
}
