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
   public class RenderTexture : Texture
   {
      public override int width
      {
         get
         {
            return Internal_GetWidth( this );
         }
         set
         {
            Internal_SetWidth( this, value );
         }
      }

      public override int height
      {
         get
         {
            return Internal_GetHeight( this );
         }
         set
         {
            Internal_SetHeight( this, value );
         }
      }

      public int depth
      {
   
   
         get;
   
   
         set;
      }

      public bool isPowerOfTwo
      {
   
   
         get;
   
   
         set;
      }

      public bool sRGB
      {
   
   
         get;
      }

      public RenderTextureFormat format
      {
   
   
         get;
   
   
         set;
      }

      public bool useMipMap
      {
   
   
         get;
   
   
         set;
      }

      public bool autoGenerateMips
      {
   
   
         get;
   
   
         set;
      }

      public override TextureDimension dimension
      {
         get
         {
            return Internal_GetDimension( this );
         }
         set
         {
            Internal_SetDimension( this, value );
         }
      }

      [Obsolete( "Use RenderTexture.dimension instead." )]
      public bool isCubemap
      {
   
   
         get;
   
   
         set;
      }

      [Obsolete( "Use RenderTexture.dimension instead." )]
      public bool isVolume
      {
   
   
         get;
   
   
         set;
      }

      public int volumeDepth
      {
   
   
         get;
   
   
         set;
      }

      public int antiAliasing
      {
   
   
         get;
   
   
         set;
      }

      public bool enableRandomWrite
      {
   
   
         get;
   
   
         set;
      }

      public RenderBuffer colorBuffer
      {
         get
         {
            GetColorBuffer( out RenderBuffer res );
            return res;
         }
      }

      public RenderBuffer depthBuffer
      {
         get
         {
            GetDepthBuffer( out RenderBuffer res );
            return res;
         }
      }

      public static RenderTexture active
      {
   
   
         get;
   
   
         set;
      }

      [Obsolete( "RenderTexture.enabled is always now, no need to use it" )]
      public static bool enabled
      {
   
   
         get;
   
   
         set;
      }

      public RenderTexture( IntPtr pointer ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public RenderTexture( int width, int height, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public RenderTexture( int width, int height, int depth, RenderTextureFormat format ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public RenderTexture( int width, int height, int depth ) : base( IntPtr.Zero ) => throw new NotImplementedException();



      private static extern void Internal_CreateRenderTexture( RenderTexture rt );



      public static extern RenderTexture GetTemporary( int width, int height, int depthBuffer, RenderTextureFormat format, RenderTextureReadWrite readWrite, int antiAliasing );


      public static RenderTexture GetTemporary( int width, int height, int depthBuffer, RenderTextureFormat format, RenderTextureReadWrite readWrite )
      {
         int antiAliasing = 1;
         return GetTemporary( width, height, depthBuffer, format, readWrite, antiAliasing );
      }


      public static RenderTexture GetTemporary( int width, int height, int depthBuffer, RenderTextureFormat format )
      {
         int antiAliasing = 1;
         RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default;
         return GetTemporary( width, height, depthBuffer, format, readWrite, antiAliasing );
      }


      public static RenderTexture GetTemporary( int width, int height, int depthBuffer )
      {
         int antiAliasing = 1;
         RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default;
         RenderTextureFormat format = RenderTextureFormat.Default;
         return GetTemporary( width, height, depthBuffer, format, readWrite, antiAliasing );
      }


      public static RenderTexture GetTemporary( int width, int height )
      {
         int antiAliasing = 1;
         RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default;
         RenderTextureFormat format = RenderTextureFormat.Default;
         int depthBuffer = 0;
         return GetTemporary( width, height, depthBuffer, format, readWrite, antiAliasing );
      }



      public static extern void ReleaseTemporary( RenderTexture temp );



      private static extern int Internal_GetWidth( RenderTexture mono );



      private static extern void Internal_SetWidth( RenderTexture mono, int width );



      private static extern int Internal_GetHeight( RenderTexture mono );



      private static extern void Internal_SetHeight( RenderTexture mono, int width );



      private static extern void Internal_SetSRGBReadWrite( RenderTexture mono, bool sRGB );



      private static extern TextureDimension Internal_GetDimension( RenderTexture rt );



      private static extern void Internal_SetDimension( RenderTexture rt, TextureDimension dim );

      public bool Create()
      {
         return INTERNAL_CALL_Create( this );
      }



      private static extern bool INTERNAL_CALL_Create( RenderTexture self );

      public void Release()
      {
         INTERNAL_CALL_Release( this );
      }



      private static extern void INTERNAL_CALL_Release( RenderTexture self );

      public bool IsCreated()
      {
         return INTERNAL_CALL_IsCreated( this );
      }



      private static extern bool INTERNAL_CALL_IsCreated( RenderTexture self );

      public void DiscardContents()
      {
         INTERNAL_CALL_DiscardContents( this );
      }



      private static extern void INTERNAL_CALL_DiscardContents( RenderTexture self );



      public extern void DiscardContents( bool discardColor, bool discardDepth );

      public void MarkRestoreExpected()
      {
         INTERNAL_CALL_MarkRestoreExpected( this );
      }



      private static extern void INTERNAL_CALL_MarkRestoreExpected( RenderTexture self );

      public void GenerateMips()
      {
         INTERNAL_CALL_GenerateMips( this );
      }



      private static extern void INTERNAL_CALL_GenerateMips( RenderTexture self );



      private extern void GetColorBuffer( out RenderBuffer res );



      private extern void GetDepthBuffer( out RenderBuffer res );

      public IntPtr GetNativeDepthBufferPtr()
      {
         INTERNAL_CALL_GetNativeDepthBufferPtr( this, out IntPtr value );
         return value;
      }



      private static extern void INTERNAL_CALL_GetNativeDepthBufferPtr( RenderTexture self, out IntPtr value );



      public extern void SetGlobalShaderProperty( string propertyName );

      [Obsolete( "GetTexelOffset always returns zero now, no point in using it." )]
      public Vector2 GetTexelOffset()
      {
         return Vector2.zero;
      }



      public static extern bool SupportsStencil( RenderTexture rt );

      [Obsolete( "SetBorderColor is no longer supported.", true )]
      public void SetBorderColor( Color color )
      {
      }
   }
}
