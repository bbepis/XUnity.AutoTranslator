using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#if IL2CPP
using Il2CppInterop.Runtime;
#endif

namespace UnityEngine
{
   public sealed class Texture2D : Texture
   {
      [Flags]
      public enum EXRFlags
      {
         None = 0x0,
         OutputAsFloat = 0x1,
         CompressZIP = 0x2,
         CompressRLE = 0x4,
         CompressPIZ = 0x8
      }

      public int mipmapCount
      {
         get;
      }

      public TextureFormat format
      {
         get;
      }

      public static Texture2D whiteTexture
      {
         get;
      }

      public static Texture2D blackTexture
      {
         get;
      }

      public Texture2D( IntPtr pointer ) : base( pointer ) => throw new NotImplementedException();

      public Texture2D( int width, int height ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public Texture2D( int width, int height, TextureFormat format, bool mipmap ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public Texture2D( int width, int height, TextureFormat format, bool mipmap, bool linear ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      internal Texture2D( int width, int height, TextureFormat format, bool mipmap, bool linear, IntPtr nativeTex ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      private static extern void Internal_Create( Texture2D mono, int width, int height, TextureFormat format, bool mipmap, bool linear, IntPtr nativeTex );

      public static Texture2D CreateExternalTexture( int width, int height, TextureFormat format, bool mipmap, bool linear, IntPtr nativeTex )
      {
         return new Texture2D( width, height, format, mipmap, linear, nativeTex );
      }

      public extern void UpdateExternalTexture( IntPtr nativeTex );

      public void SetPixel( int x, int y, Color color )
      {
         INTERNAL_CALL_SetPixel( this, x, y, ref color );
      }

      private static extern void INTERNAL_CALL_SetPixel( Texture2D self, int x, int y, ref Color color );

      public Color GetPixel( int x, int y )
      {
         INTERNAL_CALL_GetPixel( this, x, y, out Color value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetPixel( Texture2D self, int x, int y, out Color value );

      public Color GetPixelBilinear( float u, float v )
      {
         INTERNAL_CALL_GetPixelBilinear( this, u, v, out Color value );
         return value;
      }

      private static extern void INTERNAL_CALL_GetPixelBilinear( Texture2D self, float u, float v, out Color value );

#if IL2CPP
      public void SetPixels(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Color> colors ) => throw new NotImplementedException();
#else
      public void SetPixels( Color[] colors ) => throw new NotImplementedException();
#endif

      public void SetPixels( Color[] colors, int miplevel )
      {
         int num = width >> miplevel;
         if( num < 1 )
         {
            num = 1;
         }

         int num2 = height >> miplevel;
         if( num2 < 1 )
         {
            num2 = 1;
         }

         SetPixels( 0, 0, num, num2, colors, miplevel );
      }

      public extern void SetPixels( int x, int y, int blockWidth, int blockHeight, Color[] colors, int miplevel );

      public void SetPixels( int x, int y, int blockWidth, int blockHeight, Color[] colors )
      {
         int miplevel = 0;
         SetPixels( x, y, blockWidth, blockHeight, colors, miplevel );
      }

      private extern void SetAllPixels32( Color32[] colors, int miplevel );

      private extern void SetBlockOfPixels32( int x, int y, int blockWidth, int blockHeight, Color32[] colors, int miplevel );

#if IL2CPP
      public void SetPixels32(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Color32> colors ) => throw new NotImplementedException();
#else
      public void SetPixels32( Color32[] colors ) => throw new NotImplementedException();
#endif

      public void SetPixels32( Color32[] colors, int miplevel )
      {
         SetAllPixels32( colors, miplevel );
      }

      public void SetPixels32( int x, int y, int blockWidth, int blockHeight, Color32[] colors )
      {
         int miplevel = 0;
         SetPixels32( x, y, blockWidth, blockHeight, colors, miplevel );
      }

      public void SetPixels32( int x, int y, int blockWidth, int blockHeight, Color32[] colors, int miplevel )
      {
         SetBlockOfPixels32( x, y, blockWidth, blockHeight, colors, miplevel );
      }

      public extern bool LoadImage( byte[] data, bool markNonReadable );

      public bool LoadImage( byte[] data )
      {
         bool markNonReadable = false;
         return LoadImage( data, markNonReadable );
      }

      private extern void LoadRawTextureData_ImplArray( byte[] data );

      private extern void LoadRawTextureData_ImplPointer( IntPtr data, int size );

      public void LoadRawTextureData( byte[] data )
      {
         LoadRawTextureData_ImplArray( data );
      }

      public void LoadRawTextureData( IntPtr data, int size )
      {
         LoadRawTextureData_ImplPointer( data, size );
      }

      public extern byte[] GetRawTextureData();

      public Color[] GetPixels()
      {
         int miplevel = 0;
         return GetPixels( miplevel );
      }

      public Color[] GetPixels( int miplevel )
      {
         int num = width >> miplevel;
         if( num < 1 )
         {
            num = 1;
         }

         int num2 = height >> miplevel;
         if( num2 < 1 )
         {
            num2 = 1;
         }

         return GetPixels( 0, 0, num, num2, miplevel );
      }

      public extern Color[] GetPixels( int x, int y, int blockWidth, int blockHeight, int miplevel );

      public Color[] GetPixels( int x, int y, int blockWidth, int blockHeight )
      {
         int miplevel = 0;
         return GetPixels( x, y, blockWidth, blockHeight, miplevel );
      }

      public extern Color32[] GetPixels32( int miplevel );

      public Color32[] GetPixels32()
      {
         int miplevel = 0;
         return GetPixels32( miplevel );
      }

      public extern void Apply( bool updateMipmaps, bool makeNoLongerReadable );

      public void Apply( bool updateMipmaps )
      {
         bool makeNoLongerReadable = false;
         Apply( updateMipmaps, makeNoLongerReadable );
      }

      public void Apply()
      {
         bool makeNoLongerReadable = false;
         bool updateMipmaps = true;
         Apply( updateMipmaps, makeNoLongerReadable );
      }

      public extern bool Resize( int width, int height, TextureFormat format, bool hasMipMap );

      public bool Resize( int width, int height )
      {
         return Internal_ResizeWH( width, height );
      }

      private extern bool Internal_ResizeWH( int width, int height );

      public void Compress( bool highQuality )
      {
         INTERNAL_CALL_Compress( this, highQuality );
      }

      private static extern void INTERNAL_CALL_Compress( Texture2D self, bool highQuality );

      public extern Rect[] PackTextures( Texture2D[] textures, int padding, int maximumAtlasSize, bool makeNoLongerReadable );

      public Rect[] PackTextures( Texture2D[] textures, int padding, int maximumAtlasSize )
      {
         bool makeNoLongerReadable = false;
         return PackTextures( textures, padding, maximumAtlasSize, makeNoLongerReadable );
      }

      public Rect[] PackTextures( Texture2D[] textures, int padding )
      {
         bool makeNoLongerReadable = false;
         int maximumAtlasSize = 2048;
         return PackTextures( textures, padding, maximumAtlasSize, makeNoLongerReadable );
      }

      public static bool GenerateAtlas( Vector2[] sizes, int padding, int atlasSize, List<Rect> results )
      {
         if( sizes == null )
         {
            throw new ArgumentException( "sizes array can not be null" );
         }

         if( results == null )
         {
            throw new ArgumentException( "results list cannot be null" );
         }

         if( padding < 0 )
         {
            throw new ArgumentException( "padding can not be negative" );
         }

         if( atlasSize <= 0 )
         {
            throw new ArgumentException( "atlas size must be positive" );
         }

         results.Clear();
         if( sizes.Length == 0 )
         {
            return true;
         }

         GenerateAtlasInternal( sizes, padding, atlasSize, results );
         return results.Count != 0;
      }

      private static extern void GenerateAtlasInternal( Vector2[] sizes, int padding, int atlasSize, object resultList );

      public void ReadPixels( Rect source, int destX, int destY, bool recalculateMipMaps )
      {
         INTERNAL_CALL_ReadPixels( this, ref source, destX, destY, recalculateMipMaps );
      }

      public void ReadPixels( Rect source, int destX, int destY )
      {
         bool recalculateMipMaps = true;
         INTERNAL_CALL_ReadPixels( this, ref source, destX, destY, recalculateMipMaps );
      }

      private static extern void INTERNAL_CALL_ReadPixels( Texture2D self, ref Rect source, int destX, int destY, bool recalculateMipMaps );

      public extern byte[] EncodeToPNG();

      public extern byte[] EncodeToJPG( int quality );

      public byte[] EncodeToJPG()
      {
         return EncodeToJPG( 75 );
      }

      public extern byte[] EncodeToEXR( EXRFlags flags );

      public byte[] EncodeToEXR()
      {
         EXRFlags flags = EXRFlags.None;
         return EncodeToEXR( flags );
      }
   }
}
