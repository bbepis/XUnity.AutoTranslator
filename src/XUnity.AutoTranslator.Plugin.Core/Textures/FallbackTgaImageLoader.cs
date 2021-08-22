using System.IO;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Textures;

namespace XUnity.AutoTranslator.Plugin.Core.Managed.Textures
{
   class FallbackTgaImageLoader : ITextureLoader
   {
      public void Load( Texture2D texture, byte[] data )
      {
         if( texture == null && data == null ) return;

         var format = texture.format;

         using( var stream = new MemoryStream( data ) )
         using( var binaryReader = new BinaryReader( stream ) )
         {
            binaryReader.BaseStream.Seek( 12L, SeekOrigin.Begin );
            short num1 = binaryReader.ReadInt16();
            short num2 = binaryReader.ReadInt16();
            int num3 = (int)binaryReader.ReadByte();
            binaryReader.BaseStream.Seek( 1L, SeekOrigin.Current );
            Color[] colors = new Color[ (int)num1 * (int)num2 ];
            if( format == TextureFormat.RGB24 )
            {
               if( num3 == 32 )
               {
                  for( int index = 0; index < (int)num1 * (int)num2; ++index )
                  {
                     float b = binaryReader.ReadByte() / 255f;
                     float g = binaryReader.ReadByte() / 255f;
                     float r = binaryReader.ReadByte() / 255f;
                     binaryReader.ReadByte();
                     colors[ index ] = new Color( r, g, b, 1f );
                  }
               }
               else
               {
                  for( int index = 0; index < (int)num1 * (int)num2; ++index )
                  {
                     float b = binaryReader.ReadByte() / 255f;
                     float g = binaryReader.ReadByte() / 255f;
                     float r = binaryReader.ReadByte() / 255f;
                     colors[ index ] = new Color( r, g, b, 1f );
                  }
               }
            }
            else
            {
               if( num3 == 32 )
               {
                  for( int index = 0; index < (int)num1 * (int)num2; ++index )
                  {
                     float b = binaryReader.ReadByte() / 255f;
                     float g = binaryReader.ReadByte() / 255f;
                     float r = binaryReader.ReadByte() / 255f;
                     float a = binaryReader.ReadByte() / 255f;
                     colors[ index ] = new Color( r, g, b, a );
                  }
               }
               else
               {
                  for( int index = 0; index < (int)num1 * (int)num2; ++index )
                  {
                     float b = binaryReader.ReadByte() / 255f;
                     float g = binaryReader.ReadByte() / 255f;
                     float r = binaryReader.ReadByte() / 255f;
                     colors[ index ] = new Color( r, g, b, 1f );
                  }
               }
            }
            texture.SetPixels( colors );
            texture.Apply();
         }
      }

      public bool Verify()
      {
         Load( null, null ); // Verify by making sure that the method can be JITed

         return true;
      }
   }
}
