using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextureExtensions
   {
      private static readonly MethodInfo LoadImage = UnityTypes.ImageConversion != null ? AccessToolsShim.Method( UnityTypes.ImageConversion, "LoadImage", new[] { typeof( Texture2D ), typeof( byte[] ), typeof( bool ) } ) : null;
      private static readonly MethodInfo EncodeToPNG = UnityTypes.ImageConversion != null ? AccessToolsShim.Method( UnityTypes.ImageConversion, "EncodeToPNG", new[] { typeof( Texture2D ) } ) : null;

      //public static bool IsNonReadable( this Texture2D texture )
      //{
      //   return texture.GetRawTextureData().Length == 0;
      //}

      public static bool IsKnownImageType( this object ui )
      {
         var type = ui.GetType();

         return ( ui is Material || ui is SpriteRenderer )
            || ( UnityTypes.Image != null && UnityTypes.Image.IsAssignableFrom( type ) )
            || ( UnityTypes.RawImage != null && UnityTypes.RawImage.IsAssignableFrom( type ) )
            || ( UnityTypes.CubismRenderer != null && UnityTypes.CubismRenderer.IsAssignableFrom( type ) )
            || ( UnityTypes.UIWidget != null && type != UnityTypes.UILabel && UnityTypes.UIWidget.IsAssignableFrom( type ) )
            || ( UnityTypes.UIAtlas != null && UnityTypes.UIAtlas.IsAssignableFrom( type ) )
            || ( UnityTypes.UITexture != null && UnityTypes.UITexture.IsAssignableFrom( type ) )
            || ( UnityTypes.UIPanel != null && UnityTypes.UIPanel.IsAssignableFrom( type ) );
      }

      public static string GetTextureName( this Texture texture )
      {
         if( !string.IsNullOrEmpty( texture.name ) ) return texture.name;
         return "Unnamed";
      }

      public static void LoadImageEx( this Texture2D texture, byte[] data )
      {
         if( LoadImage != null )
         {
            LoadImage.Invoke( null, new object[] { texture, data, false } );
         }
         else
         {
            texture.LoadImageSafe( data );
         }
      }

      private static void LoadImageSafe( this Texture2D texture, byte[] data )
      {
         texture.LoadImage( data ); // markNonReadable always false in this plugin anyway; causes problems in old unity version
      }

      public static byte[] EncodeToPNGEx( this Texture2D texture )
      {
         if( EncodeToPNG != null )
         {
            return (byte[])EncodeToPNG.Invoke( null, new object[] { texture } );
         }
         else
         {
            return texture.EncodeToPNGSafe();
         }
      }

      private static byte[] EncodeToPNGSafe( this Texture2D texture )
      {
         return texture.EncodeToPNG();
      }
   }
}
