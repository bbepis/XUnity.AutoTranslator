using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class TextureTranslationInfo
   {
      //private static readonly Dictionary<int, string> KnownHashes = new Dictionary<int, string>();
      private static readonly Encoding UTF8 = new UTF8Encoding( false );

      private string _key;
      private byte[] _originalData;
      private bool? _nonReadable;

      public bool IsTranslated { get; set; }

      public bool HasDumpedAlternativeTexture { get; set; }

      public bool IsNonReadable( Texture2D texture )
      {
         if( !_nonReadable.HasValue )
         {
            _nonReadable = false;
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
            var instanceId = texture.GetInstanceID();

            if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageData )
            {
               var result = TextureHelper.GetData( texture );

               _originalData = result.Data;
               _nonReadable = result.NonReadable;
               _key = HashHelper.Compute( _originalData );

               //if( KnownHashes.TryGetValue( instanceId, out string hashValue ) )
               //{
               //   _key = hashValue;

               //   if( Settings.EnableTextureToggling )
               //   {
               //      var result = TextureHelper.GetData( texture );

               //      _originalData = result.Data;
               //      _nonReadable = result.NonReadable;
               //   }
               //}
               //else
               //{
               //   var result = TextureHelper.GetData( texture );

               //   _originalData = result.Data;
               //   _nonReadable = result.NonReadable;
               //   _key = HashHelper.Compute( _originalData );

               //   if( !string.IsNullOrEmpty( texture.name ) && result.CalculationTime > 0.6f )
               //   {
               //      KnownHashes[ instanceId ] = _key;
               //   }
               //}
            }
            else if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageName )
            {
               var name = texture.name; // name may be duplicate, WILL be duplicate!
               if( string.IsNullOrEmpty( name ) ) return;

               _key = HashHelper.Compute( UTF8.GetBytes( name ) );

               if( Settings.EnableTextureToggling )
               {
                  var result = TextureHelper.GetData( texture );

                  _originalData = result.Data;
                  _nonReadable = result.NonReadable;
               }
            }
            else if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageNameAndScene )
            {
               var name = texture.name; // name may be duplicate, WILL be duplicate!
               if( string.IsNullOrEmpty( name ) ) return;

               name += "|" + SceneManagerEx.GetActiveSceneId();

               _key = HashHelper.Compute( UTF8.GetBytes( name ) );

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
