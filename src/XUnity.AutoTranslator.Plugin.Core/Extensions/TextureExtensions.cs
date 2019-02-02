using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextureExtensions
   {
      private static readonly MethodInfo LoadImage = AccessTools.Method( ClrTypes.ImageConversion, "LoadImage", new[] { typeof( Texture2D ), typeof( byte[] ), typeof( bool ) } );
      private static readonly MethodInfo EncodeToPNG = AccessTools.Method( ClrTypes.ImageConversion, "EncodeToPNG", new[] { typeof( Texture2D ) } );

      //public static bool IsNonReadable( this Texture2D texture )
      //{
      //   return texture.GetRawTextureData().Length == 0;
      //}

      public static bool IsKnownImageType( this object ui )
      {
         var type = ui.GetType();

         return ( ui is Material || ui is UnityEngine.UI.Image || ui is RawImage || ui is SpriteRenderer )
            || ( ClrTypes.CubismRenderer != null && ClrTypes.CubismRenderer.IsAssignableFrom( type ) )
            || ( ClrTypes.UIWidget != null && type != ClrTypes.UILabel && ClrTypes.UIWidget.IsAssignableFrom( type ) )
            || ( ClrTypes.UIAtlas != null && ClrTypes.UIAtlas.IsAssignableFrom( type ) )
            || ( ClrTypes.UITexture != null && ClrTypes.UITexture.IsAssignableFrom( type ) )
            || ( ClrTypes.UIPanel != null && ClrTypes.UIPanel.IsAssignableFrom( type ) );
      }

      public static string GetTextureName( this Texture texture )
      {
         if( !string.IsNullOrEmpty( texture.name ) ) return texture.name;
         return "Unnamed";
      }

      public static void LoadImageEx( this Texture2D texture, byte[] data, bool markNonReadable )
      {
         if( LoadImage != null )
         {
            LoadImage.Invoke( null, new object[] { texture, data, markNonReadable } );
         }
         else
         {
            texture.LoadImageSafe( data, markNonReadable );
         }
      }

      private static void LoadImageSafe( this Texture2D texture, byte[] data, bool markNonReadable )
      {
         texture.LoadImage( data, markNonReadable );
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
