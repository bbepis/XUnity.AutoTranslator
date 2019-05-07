using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TextureTranslationInfo
   {
      private static Dictionary<string, string> NameToHash = new Dictionary<string, string>();
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

      private TextureDataResult SetupKeyForNameWithFallback( string name, Texture2D texture )
      {
         bool detectedDuplicateName = false;
         string existingHash = null;
         string hash = null;

         TextureDataResult result = null;

         if( Settings.DetectDuplicateTextureNames )
         {
            result = TextureHelper.GetData( texture );
            hash = HashHelper.Compute( result.Data );

            if( NameToHash.TryGetValue( name, out existingHash ) )
            {
               if( existingHash != hash )
               {
                  XuaLogger.Current.Warn( "Detected duplicate image name: " + name );
                  detectedDuplicateName = true;

                  Settings.AddDuplicateName( name );
               }
            }
            else
            {
               NameToHash[ name ] = hash;
            }
         }

         if( Settings.DuplicateTextureNames.Contains( name ) )
         {
            if( hash == null )
            {
               if( result == null )
               {
                  result = TextureHelper.GetData( texture );
               }

               hash = HashHelper.Compute( result.Data );
            }

            _key = hash;
         }
         else
         {
            _key = HashHelper.Compute( UTF8.GetBytes( name ) );
         }

         if( detectedDuplicateName && Settings.EnableTextureDumping )
         {
            var oldKey = HashHelper.Compute( UTF8.GetBytes( name ) );
            AutoTranslationPlugin.Current.RenameTextureWithKey( name, oldKey, existingHash );
         }

         return result;
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
               _key = HashHelper.Compute( _originalData );
            }
            else if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageName )
            {
               var name = texture.name; // name may be duplicate, WILL be duplicate!
               if( string.IsNullOrEmpty( name ) ) return;

               var result = SetupKeyForNameWithFallback( name, texture );

               if( Settings.EnableTextureToggling || Settings.DetectDuplicateTextureNames )
               {
                  if( result == null )
                  {
                     result = TextureHelper.GetData( texture );
                  }

                  _originalData = result.Data;
                  _nonReadable = result.NonReadable;
               }
            }
            else if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageNameAndScene )
            {
               var name = texture.name; // name may be duplicate, WILL be duplicate!
               if( string.IsNullOrEmpty( name ) ) return;

               name += "|" + SceneManagerHelper.GetActiveSceneId();

               var result = SetupKeyForNameWithFallback( name, texture );

               if( Settings.EnableTextureToggling || Settings.DetectDuplicateTextureNames )
               {
                  if( result == null )
                  {
                     result = TextureHelper.GetData( texture );
                  }

                  _originalData = result.Data;
                  _nonReadable = result.NonReadable;
               }
            }
         }
      }
   }
}
