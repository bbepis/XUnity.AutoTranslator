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
      private static readonly MethodInfo LoadImage = ClrTypes.ImageConversion != null ? AccessToolsShim.Method( ClrTypes.ImageConversion, "LoadImage", new[] { typeof( Texture2D ), typeof( byte[] ), typeof( bool ) } ) : null;
      private static readonly MethodInfo EncodeToPNG = ClrTypes.ImageConversion != null ? AccessToolsShim.Method( ClrTypes.ImageConversion, "EncodeToPNG", new[] { typeof( Texture2D ) } ) : null;

      //public static bool IsNonReadable( this Texture2D texture )
      //{
      //   return texture.GetRawTextureData().Length == 0;
      //}

      public static bool IsKnownImageType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( ui is Material || ui is SpriteRenderer )
            || ( ClrTypes.Image != null && ClrTypes.Image.IsAssignableFrom( type ) )
            || ( ClrTypes.RawImage != null && ClrTypes.RawImage.IsAssignableFrom( type ) )
            || ( ClrTypes.CubismRenderer != null && ClrTypes.CubismRenderer.IsAssignableFrom( type ) )
            || ( ClrTypes.UIWidget != null && type != ClrTypes.UILabel && ClrTypes.UIWidget.IsAssignableFrom( type ) )
            || ( ClrTypes.UIAtlas != null && ClrTypes.UIAtlas.IsAssignableFrom( type ) )
            || ( ClrTypes.UITexture != null && ClrTypes.UITexture.IsAssignableFrom( type ) )
            || ( ClrTypes.UIPanel != null && ClrTypes.UIPanel.IsAssignableFrom( type ) );
      }

      public static string GetTextureName( this Texture texture )
      {
         var name = texture.name;

         if( !string.IsNullOrEmpty( name ) ) return name;
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
