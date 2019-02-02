using Harmony;
using UnityEngine;
using UnityEngine.UI;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextureComponentExtensions
   {
      private static readonly string TexturePropertyName = "texture";
      private static readonly string MainTexturePropertyName = "mainTexture";
      private static readonly string CapitalMainTexturePropertyName = "MainTexture";
      private static readonly string MarkAsChangedMethodName = "MarkAsChanged";

      public static Texture2D GetTexture( this object ui )
      {
         if( ui == null ) return null;

         if( ui is Image image )
         {
            return image.mainTexture as Texture2D;
         }
         else if( ui is RawImage rawImage )
         {
            return rawImage.mainTexture as Texture2D;
         }
         else if( ui is SpriteRenderer spriteRenderer )
         {
            return spriteRenderer.sprite?.texture;
         }
         else
         {
            // lets attempt some reflection for several known types
            var type = ui.GetType();
            var texture = type.GetProperty( MainTexturePropertyName )?.GetValue( ui, null )
               ?? type.GetProperty( TexturePropertyName )?.GetValue( ui, null )
               ?? type.GetProperty( CapitalMainTexturePropertyName )?.GetValue( ui, null );

            return texture as Texture2D;
         }
      }

      public static void SetAllDirtyEx( this object ui )
      {
         if( ui == null ) return;

         if( ui is Graphic graphic )
         {
            graphic.SetAllDirty();
         }
         else
         {
            // lets attempt some reflection for several known types
            var type = ui.GetType();

            AccessTools.Method( type, MarkAsChangedMethodName )?.Invoke( ui, null );
         }
      }
   }
}
