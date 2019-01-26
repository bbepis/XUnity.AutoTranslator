using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class TextureExtensions
   {
      private static readonly MethodInfo LoadImage = AccessTools.Method( ClrTypes.ImageConversion, "LoadImage", new[] { typeof( Texture2D ), typeof( byte[] ), typeof( bool ) } );
      private static readonly MethodInfo EncodeToPNG = AccessTools.Method( ClrTypes.ImageConversion, "EncodeToPNG", new[] { typeof( Texture2D ) } );

      //public static bool IsNonReadable( this Texture2D texture )
      //{
      //   return texture.GetRawTextureData().Length == 0;
      //}

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
