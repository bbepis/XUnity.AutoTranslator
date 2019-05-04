using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   class TextureTranslationCache
   {
      private Dictionary<string, byte[]> _translatedImages = new Dictionary<string, byte[]>( StringComparer.InvariantCultureIgnoreCase );
      private HashSet<string> _untranslatedImages = new HashSet<string>();
      private Dictionary<string, string> _keyToFileName = new Dictionary<string, string>();

      public TextureTranslationCache()
      {

      }

      private IEnumerable<string> GetTextureFiles()
      {
         return Directory.GetFiles( Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TextureDirectory ).Parameterize(), $"*.png", SearchOption.AllDirectories )
            .Select( x => x.Replace( "/", "\\" ) );
      }

      public void LoadTranslationFiles()
      {
         try
         {
            if( Settings.EnableTextureTranslation || Settings.EnableTextureDumping )
            {
               var startTime = Time.realtimeSinceStartup;

               _translatedImages.Clear();
               _untranslatedImages.Clear();
               _keyToFileName.Clear();

               Directory.CreateDirectory( Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TextureDirectory ).Parameterize() );
               foreach( var fullFileName in GetTextureFiles() )
               {
                  RegisterImageFromFile( fullFileName );
               }

               var endTime = Time.realtimeSinceStartup;
               XuaLogger.Current.Info( $"Loaded texture files (took {Math.Round( endTime - startTime, 2 )} seconds)" );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while loading translations." );
         }
      }

      private void RegisterImageFromFile( string fullFileName )
      {
         try
         {
            var fileName = Path.GetFileNameWithoutExtension( fullFileName );
            var startHash = fileName.LastIndexOf( "[" );
            var endHash = fileName.LastIndexOf( "]" );

            if( endHash > -1 && startHash > -1 && endHash > startHash )
            {
               var takeFrom = startHash + 1;

               // load based on whether or not the key is image hashed
               var parts = fileName.Substring( takeFrom, endHash - takeFrom ).Split( '-' );
               string key;
               string originalHash;
               if( parts.Length == 1 )
               {
                  key = parts[ 0 ];
                  originalHash = parts[ 0 ];
               }
               else if( parts.Length == 2 )
               {
                  key = parts[ 0 ];
                  originalHash = parts[ 1 ];
               }
               else
               {
                  XuaLogger.Current.Warn( $"Image not loaded (unknown hash): {fullFileName}." );
                  return;
               }

               var data = File.ReadAllBytes( fullFileName );
               var currentHash = HashHelper.Compute( data );
               var isModified = StringComparer.InvariantCultureIgnoreCase.Compare( originalHash, currentHash ) != 0;

               _keyToFileName[ key ] = fullFileName;

               // only load images that someone has modified!
               if( Settings.LoadUnmodifiedTextures || isModified )
               {
                  RegisterTranslatedImage( key, data );
                  XuaLogger.Current.Debug( $"Image loaded: {fullFileName}." );
               }
               else
               {
                  RegisterUntranslatedImage( key );
                  XuaLogger.Current.Warn( $"Image not loaded (unmodified): {fullFileName}." );
               }
            }
            else
            {
               XuaLogger.Current.Warn( $"Image not loaded (no hash): {fullFileName}." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while loading texture file: " + fullFileName );
         }
      }

      public void RenameFileWithKey( string name, string key, string newKey )
      {
         try
         {
            if( _keyToFileName.TryGetValue( key, out var currentFileName ) )
            {
               _keyToFileName.Remove( key );
               var newFileName = currentFileName.Replace( key, newKey );

               if( !IsImageRegistered( newKey ) )
               {
                  var data = File.ReadAllBytes( currentFileName );
                  RegisterImageFromData( name, newKey, data );
                  File.Delete( currentFileName );

                  XuaLogger.Current.Warn( $"Replaced old file with name '{name}' registered with key old '{key}'." );
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, $"An error occurred while trying to rename file with key '{key}'." );
         }
      }

      internal void RegisterImageFromData( string textureName, string key, byte[] data )
      {
         var name = textureName.SanitizeForFileSystem();
         var root = Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TextureDirectory ).Parameterize();
         var originalHash = HashHelper.Compute( data );

         // allow hash and key to be the same; only store one of them then!
         string fileName;
         if( key == originalHash )
         {
            fileName = name + " [" + key + "].png";
         }
         else
         {
            fileName = name + " [" + key + "-" + originalHash + "].png";
         }

         var fullName = Path.Combine( root, fileName );
         File.WriteAllBytes( fullName, data );
         XuaLogger.Current.Info( "Dumped texture file: " + fileName );

         _keyToFileName[ key ] = fullName;

         if( Settings.LoadUnmodifiedTextures )
         {
            RegisterTranslatedImage( key, data );
         }
         else
         {
            RegisterUntranslatedImage( key );
         }
      }

      private void RegisterTranslatedImage( string key, byte[] data )
      {
         _translatedImages[ key ] = data;
      }

      private void RegisterUntranslatedImage( string key )
      {
         _untranslatedImages.Add( key );
      }

      internal bool IsImageRegistered( string key )
      {
         return _translatedImages.ContainsKey( key ) || _untranslatedImages.Contains( key );
      }

      internal bool TryGetTranslatedImage( string key, out byte[] data )
      {
         return _translatedImages.TryGetValue( key, out data );
      }
   }
}
