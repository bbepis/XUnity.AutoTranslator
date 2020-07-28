using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Shims;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TextureTranslationInfo
   {
      private static Dictionary<string, string> NameToHash = new Dictionary<string, string>();
      private static readonly Encoding UTF8 = new UTF8Encoding( false );

      private string _key;
      private byte[] _originalData;

      public WeakReference Original { get; private set; }

      public object Translated { get; private set; }

      public bool IsTranslated { get; set; }

      public bool IsDumped { get; set; }

      public void SetOriginal( object texture )
      {
         Original = new WeakReference( texture );
      }

      private void SetTranslated( object texture )
      {
         Translated = texture;
      }

      public void CreateTranslatedTexture( byte[] newData )
      {
         if( Translated == null )
         {
            var orig = Original.Target;

            var texture = ComponentHelper.Instance.CreateEmptyTexture2D();
            texture.LoadImageEx( newData, orig );

            SetTranslated( texture );

            texture.SetExtensionData( this );
         }
      }

      public void CreateOriginalTexture()
      {
         if( !Original.IsAlive && _originalData != null )
         {
            var texture = ComponentHelper.Instance.CreateEmptyTexture2D();
            texture.LoadImageEx( _originalData, null );

            SetOriginal( texture );
         }
      }

      public string GetKey()
      {
         SetupHashAndData( Original.Target );
         return _key;
      }

      public byte[] GetOriginalData()
      {
         SetupHashAndData( Original.Target );
         return _originalData;
      }

      public byte[] GetOrCreateOriginalData()
      {
         // special handling if SetupHashAndData is changed to not support originalData
         // which frankly, is a memory drain

         SetupHashAndData( Original.Target );
         if( _originalData != null ) return _originalData;
         return Original.Target.GetTextureData().Data;
      }

      private TextureDataResult SetupKeyForNameWithFallback( string name, object texture )
      {
         bool detectedDuplicateName = false;
         string existingHash = null;
         string hash = null;

         TextureDataResult result = null;

         if( Settings.DetectDuplicateTextureNames )
         {
            result = texture.GetTextureData();
            hash = HashHelper.Compute( result.Data );

            if( NameToHash.TryGetValue( name, out existingHash ) )
            {
               if( existingHash != hash )
               {
                  XuaLogger.AutoTranslator.Warn( "Detected duplicate image name: " + name );
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
                  result = texture.GetTextureData();
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
            AutoTranslator.Internal.TextureCache.RenameFileWithKey( name, oldKey, existingHash );
         }

         return result;
      }

      private void SetupHashAndData( object texture )
      {
         if( _key == null )
         {
            if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageData )
            {
               var result = texture.GetTextureData();

               _originalData = result.Data;
               _key = HashHelper.Compute( _originalData );
            }
            else if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageName )
            {
               var name = texture.GetTextureName( null ); // name may be duplicate, WILL be duplicate!
               if( name == null ) return;

               var result = SetupKeyForNameWithFallback( name, texture );

               if( Settings.EnableTextureToggling || Settings.DetectDuplicateTextureNames )
               {
                  if( result == null )
                  {
                     result = texture.GetTextureData();
                  }

                  _originalData = result.Data;
               }
            }
            else if( Settings.TextureHashGenerationStrategy == TextureHashGenerationStrategy.FromImageNameAndScene )
            {
               var name = texture.GetTextureName( null ); // name may be duplicate, WILL be duplicate!
               if( name == null ) return;

               name += "|" + TranslationScopeHelper.Instance.GetActiveSceneId().ToString();

               var result = SetupKeyForNameWithFallback( name, texture );

               if( Settings.EnableTextureToggling || Settings.DetectDuplicateTextureNames )
               {
                  if( result == null )
                  {
                     result = texture.GetTextureData();
                  }

                  _originalData = result.Data;
               }
            }
         }
      }
   }
}
