using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   public static class TextureHelper
   {
      private static readonly Color Transparent = new Color( 0, 0, 0, 0 );
      
      public static TextureDataResult GetData( Texture2D texture, RenderTextureFormat rtf = RenderTextureFormat.Default, RenderTextureReadWrite cs = RenderTextureReadWrite.Default )
      {
         var start = Time.realtimeSinceStartup;

         byte[] data = null;
         //bool nonReadable = texture.IsNonReadable();

         //if( !nonReadable )
         //{
         //   data = texture.EncodeToPNGEx();
         //}

         if( data == null )
         {
            var tmp = RenderTexture.GetTemporary( texture.width, texture.height, 0, rtf, cs );
            GL.Clear( false, true, Transparent );
            Graphics.Blit( texture, tmp );
            var previousRenderTexture = RenderTexture.active;
            RenderTexture.active = tmp;

            var texture2d = new Texture2D( texture.width, texture.height );
            texture2d.ReadPixels( new Rect( 0, 0, tmp.width, tmp.height ), 0, 0 );
            data = texture2d.EncodeToPNGEx();
            UnityEngine.Object.DestroyImmediate( texture2d );

            //Graphics.Blit( tex, tmp );
            //var texture2d = GetTextureFromRenderTexture( tmp );
            //var data = texture2d.EncodeToPNG();
            //UnityEngine.Object.DestroyImmediate( texture2d );

            RenderTexture.active = previousRenderTexture == tmp ? null : previousRenderTexture;
            RenderTexture.ReleaseTemporary( tmp );
         }

         var end = Time.realtimeSinceStartup;

         return new TextureDataResult( data, false, end - start );
      }
   }

   public struct TextureDataResult
   {
      public TextureDataResult( byte[] data, bool nonReadable, float calculationTime )
      {
         Data = data;
         NonReadable = nonReadable;
         CalculationTime = calculationTime;
      }

      public byte[] Data { get; }

      public bool NonReadable { get; }

      public float CalculationTime { get; set; }
   }
}
