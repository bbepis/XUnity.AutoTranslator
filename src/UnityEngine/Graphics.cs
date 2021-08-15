using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   public sealed class Graphics
   {
      public static RenderBuffer activeColorBuffer
      {
         get
         {
            GetActiveColorBuffer( out RenderBuffer res );
            return res;
         }
      }

      public static RenderBuffer activeDepthBuffer
      {
         get
         {
            GetActiveDepthBuffer( out RenderBuffer res );
            return res;
         }
      }

      public static GraphicsTier activeTier
      {
         get;
         set;
      }

      private static extern Array ExtractArrayFromList( object list );



      public static void DrawTexture( Rect screenRect, Texture texture, Material mat )
      {
         int pass = -1;
         DrawTexture( screenRect, texture, mat, pass );
      }


      public static void DrawTexture( Rect screenRect, Texture texture )
      {
         int pass = -1;
         Material mat = null;
         DrawTexture( screenRect, texture, mat, pass );
      }

      public static void DrawTexture( Rect screenRect, Texture texture, Material mat, int pass )
      {
         DrawTexture( screenRect, texture, 0, 0, 0, 0, mat, pass );
      }


      public static void DrawTexture( Rect screenRect, Texture texture, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Material mat )
      {
         int pass = -1;
         DrawTexture( screenRect, texture, leftBorder, rightBorder, topBorder, bottomBorder, mat, pass );
      }


      public static void DrawTexture( Rect screenRect, Texture texture, int leftBorder, int rightBorder, int topBorder, int bottomBorder )
      {
         int pass = -1;
         Material mat = null;
         DrawTexture( screenRect, texture, leftBorder, rightBorder, topBorder, bottomBorder, mat, pass );
      }

      public static void DrawTexture( Rect screenRect, Texture texture, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Material mat, int pass )
      {
         DrawTexture( screenRect, texture, new Rect( 0f, 0f, 1f, 1f ), leftBorder, rightBorder, topBorder, bottomBorder, mat, pass );
      }


      public static void DrawTexture( Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Material mat )
      {
         int pass = -1;
         DrawTexture( screenRect, texture, sourceRect, leftBorder, rightBorder, topBorder, bottomBorder, mat, pass );
      }


      public static void DrawTexture( Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder )
      {
         int pass = -1;
         Material mat = null;
         DrawTexture( screenRect, texture, sourceRect, leftBorder, rightBorder, topBorder, bottomBorder, mat, pass );
      }

      public static void DrawTexture( Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Material mat, int pass ) => throw new NotImplementedException();


      public static void DrawTexture( Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Color color, Material mat )
      {
         int pass = -1;
         DrawTexture( screenRect, texture, sourceRect, leftBorder, rightBorder, topBorder, bottomBorder, color, mat, pass );
      }


      public static void DrawTexture( Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Color color )
      {
         int pass = -1;
         Material mat = null;
         DrawTexture( screenRect, texture, sourceRect, leftBorder, rightBorder, topBorder, bottomBorder, color, mat, pass );
      }

      public static void DrawTexture( Rect screenRect, Texture texture, Rect sourceRect, int leftBorder, int rightBorder, int topBorder, int bottomBorder, Color color, Material mat, int pass ) => throw new NotImplementedException();







      public static extern void Blit( Texture source, RenderTexture dest );


      public static void Blit( Texture source, RenderTexture dest, Material mat )
      {
         int pass = -1;
         Blit( source, dest, mat, pass );
      }

      public static void Blit( Texture source, RenderTexture dest, Material mat, int pass )
      {
         Internal_BlitMaterial( source, dest, mat, pass, setRT: true );
      }


      public static void Blit( Texture source, Material mat )
      {
         int pass = -1;
         Blit( source, mat, pass );
      }

      public static void Blit( Texture source, Material mat, int pass )
      {
         Internal_BlitMaterial( source, null, mat, pass, setRT: false );
      }



      private static extern void Internal_BlitMaterial( Texture source, RenderTexture dest, Material mat, int pass, bool setRT );

      public static void BlitMultiTap( Texture source, RenderTexture dest, Material mat, params Vector2[] offsets )
      {
         Internal_BlitMultiTap( source, dest, mat, offsets );
      }



      private static extern void Internal_BlitMultiTap( Texture source, RenderTexture dest, Material mat, Vector2[] offsets );



      private static extern void CopyTexture_Full( Texture src, Texture dst );



      private static extern void CopyTexture_Slice_AllMips( Texture src, int srcElement, Texture dst, int dstElement );



      private static extern void CopyTexture_Slice( Texture src, int srcElement, int srcMip, Texture dst, int dstElement, int dstMip );



      private static extern void CopyTexture_Region( Texture src, int srcElement, int srcMip, int srcX, int srcY, int srcWidth, int srcHeight, Texture dst, int dstElement, int dstMip, int dstX, int dstY );



      private static extern bool ConvertTexture_Full( Texture src, Texture dst );



      private static extern bool ConvertTexture_Slice( Texture src, int srcElement, Texture dst, int dstElement );



      private static extern void Internal_SetNullRT();





      private static extern void GetActiveColorBuffer( out RenderBuffer res );



      private static extern void GetActiveDepthBuffer( out RenderBuffer res );

      public static void SetRandomWriteTarget( int index, RenderTexture uav )
      {
         Internal_SetRandomWriteTargetRT( index, uav );
      }




      public static extern void ClearRandomWriteTargets();



      private static extern void Internal_SetRandomWriteTargetRT( int index, RenderTexture uav );



      internal static void CheckLoadActionValid( RenderBufferLoadAction load, string bufferType )
      {
      }

      internal static void CheckStoreActionValid( RenderBufferStoreAction store, string bufferType )
      {
      }

      public static void SetRenderTarget( RenderTexture rt ) => throw new NotImplementedException();

      public static void SetRenderTarget( RenderTexture rt, int mipLevel ) => throw new NotImplementedException();

      public static void SetRenderTarget( RenderBuffer colorBuffer, RenderBuffer depthBuffer ) => throw new NotImplementedException();

      public static void SetRenderTarget( RenderBuffer colorBuffer, RenderBuffer depthBuffer, int mipLevel ) => throw new NotImplementedException();


      public static void SetRenderTarget( RenderBuffer[] colorBuffers, RenderBuffer depthBuffer ) => throw new NotImplementedException();


      public static void CopyTexture( Texture src, Texture dst )
      {
         CopyTexture_Full( src, dst );
      }

      public static void CopyTexture( Texture src, int srcElement, Texture dst, int dstElement )
      {
         CopyTexture_Slice_AllMips( src, srcElement, dst, dstElement );
      }

      public static void CopyTexture( Texture src, int srcElement, int srcMip, Texture dst, int dstElement, int dstMip )
      {
         CopyTexture_Slice( src, srcElement, srcMip, dst, dstElement, dstMip );
      }

      public static void CopyTexture( Texture src, int srcElement, int srcMip, int srcX, int srcY, int srcWidth, int srcHeight, Texture dst, int dstElement, int dstMip, int dstX, int dstY )
      {
         CopyTexture_Region( src, srcElement, srcMip, srcX, srcY, srcWidth, srcHeight, dst, dstElement, dstMip, dstX, dstY );
      }

      public static bool ConvertTexture( Texture src, Texture dst )
      {
         return ConvertTexture_Full( src, dst );
      }

      public static bool ConvertTexture( Texture src, int srcElement, Texture dst, int dstElement )
      {
         return ConvertTexture_Slice( src, srcElement, dst, dstElement );
      }

   }
}
