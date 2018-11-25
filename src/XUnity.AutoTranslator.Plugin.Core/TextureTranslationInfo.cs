using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class TextureTranslationInfo
   {
      private string _key;
      private byte[] _originalData;
      private bool? _nonReadable;

      public bool IsTranslated { get; set; }

      public bool HasDumpedAlternativeTexture { get; set; }

      public bool IsNonReadable( Texture2D texture )
      {
         if( !_nonReadable.HasValue )
         {
            _nonReadable = texture.IsNonReadable();
         }

         return _nonReadable.Value;
      }

      public string GetKey( Texture2D texture )
      {
         SetupHashAndData( texture );
         return _key;
      }

      public byte[] GetOriginalData( Texture2D texture )
      {
         SetupHashAndData( texture );
         return _originalData;
      }

      public byte[] GetOrCreateOriginalData( Texture2D texture )
      {
         // special handling if SetupHashAndData is changed to not support originalData
         // which frankly, is a memory drain

         SetupHashAndData( texture );
         if( _originalData != null ) return _originalData;
         return TextureHelper.GetData( texture ).Data;
      }

      private void SetupHashAndData( Texture2D texture )
      {
         if( _key == null )
         {
            if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageData )
            {
               var result = TextureHelper.GetData( texture );

               _originalData = result.Data;
               _nonReadable = result.NonReadable;
               _key = HashHelper.Compute( _originalData ).Substring( 0, 8 );
            }
            else if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageName )
            {
               var name = texture.name; // name may be duplicate, WILL be duplicate!
               if( string.IsNullOrEmpty( name ) || name.Contains( "(Clone)" ) ) return;

               _key = HashHelper.Compute( Encoding.UTF8.GetBytes( name ) ).Substring( 0, 8 );

               if( Settings.EnableTextureToggling )
               {
                  var result = TextureHelper.GetData( texture );

                  _originalData = result.Data;
                  _nonReadable = result.NonReadable;
               }
            }
            else // if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageNameThenData )
            {
               var name = texture.name;
               if( string.IsNullOrEmpty( name ) )
               {
                  var result = TextureHelper.GetData( texture );

                  _originalData = result.Data;
                  _nonReadable = result.NonReadable;
                  _key = HashHelper.Compute( _originalData ).Substring( 0, 8 );
               }
               else
               {
                  _key = HashHelper.Compute( Encoding.UTF8.GetBytes( name ) ).Substring( 0, 8 );

                  if( Settings.EnableTextureToggling )
                  {
                     var result = TextureHelper.GetData( texture );

                     _originalData = result.Data;
                     _nonReadable = result.NonReadable;
                  }
               }
            }
         }
      }
   }
}
