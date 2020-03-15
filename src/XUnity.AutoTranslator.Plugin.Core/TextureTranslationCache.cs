using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   interface ITranslatedImageSource
   {
      byte[] GetData();
   }

   class ZipFileTranslatedImageSource : ITranslatedImageSource
   {
      private readonly ZipFile _zipFile;
      private readonly ZipEntry _zipEntry;

      public ZipFileTranslatedImageSource( ZipFile zipFile, ZipEntry zipEntry )
      {
         _zipFile = zipFile;
         _zipEntry = zipEntry;
      }

      public byte[] GetData()
      {
         using( var stream = _zipFile.GetInputStream( _zipEntry ) )
         {
            return stream.ReadFully( 16 * 1024 );
         }
      }
   }

   class FileSystemTranslatedImageSource : ITranslatedImageSource
   {
      private readonly string _fileName;

      public FileSystemTranslatedImageSource( string fileName )
      {
         _fileName = fileName;
      }

      public byte[] GetData()
      {
         using( var stream = File.OpenRead( _fileName ) )
         {
            return stream.ReadFully( 16 * 1024 );
         }
      }
   }

   class TranslatedImage
   {
      private readonly ITranslatedImageSource _source;
      private WeakReference<byte[]> _weakData;
      private byte[] _data;

      public TranslatedImage( string fileName, byte[] data, ITranslatedImageSource source )
      {
         _source = source;

         FileName = fileName;
         Data = data;
      }

      public string FileName { get; }

      private byte[] Data
      {
         set
         {
            if( _source == null )
            {
               _data = value;
            }
            else
            {
               _weakData = WeakReference<byte[]>.Create( value );
            }
         }
         get
         {
            if( _source == null )
            {
               return _data;
            }
            else
            {
               var target = _weakData.Target;
               if( target != null )
               {
                  return target;
               }
               return null;
            }
         }
      }

      public byte[] GetData()
      {
         var data = Data;
         if( data != null )
         {
            return data;
         }

         if( _source != null )
         {
            data = _source.GetData();
            Data = data;

            if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Debug( $"Image loaded: {FileName}." );
         }

         return data;
      }
   }

   class TextureTranslationCache
   {
      private Dictionary<string, TranslatedImage> _translatedImages = new Dictionary<string, TranslatedImage>( StringComparer.InvariantCultureIgnoreCase );
      private HashSet<string> _untranslatedImages = new HashSet<string>();
      private Dictionary<string, string> _keyToFileName = new Dictionary<string, string>();

      public TextureTranslationCache()
      {

      }

      private IEnumerable<string> GetTextureFiles()
      {
         return Directory.GetFiles( Settings.TexturesPath, $"*.*", SearchOption.AllDirectories )
            .Where( x => x.EndsWith( ".png", StringComparison.OrdinalIgnoreCase ) || x.EndsWith( ".zip", StringComparison.OrdinalIgnoreCase ) );
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

               Directory.CreateDirectory( Settings.TexturesPath );
               foreach( var fullFileName in GetTextureFiles() )
               {
                  RegisterImageFromFile( fullFileName );
               }

               var endTime = Time.realtimeSinceStartup;
               XuaLogger.AutoTranslator.Debug( $"Loaded texture files (took {Math.Round( endTime - startTime, 2 )} seconds)" );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while loading translations." );
         }
      }

      private void RegisterImageFromStream( string fullFileName, ITranslatedImageSource source )
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
                  XuaLogger.AutoTranslator.Warn( $"Image not loaded (unknown hash): {fullFileName}." );
                  return;
               }

               var data = source.GetData();
               var currentHash = HashHelper.Compute( data );
               var isModified = StringComparer.InvariantCultureIgnoreCase.Compare( originalHash, currentHash ) != 0;

               _keyToFileName[ key ] = fullFileName;

               // only load images that someone has modified!
               if( Settings.LoadUnmodifiedTextures || isModified )
               {
                  RegisterTranslatedImage( fullFileName, key, data, source );

                  if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Debug( $"Image loaded: {fullFileName}." );
               }
               else
               {
                  RegisterUntranslatedImage( key );
                  XuaLogger.AutoTranslator.Warn( $"Image not loaded (unmodified): {fullFileName}." );
               }
            }
            else
            {
               XuaLogger.AutoTranslator.Warn( $"Image not loaded (no hash): {fullFileName}." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while loading texture file: " + fullFileName );
         }
      }

      private void RegisterImageFromFile( string fullFileName )
      {
         var fileExists = File.Exists( fullFileName );
         if( fileExists )
         {
            // Perhaps use this instead???? https://github.com/icsharpcode/SharpZipLib/wiki/Unpack-a-zip-using-ZipInputStream
            if( fullFileName.EndsWith( ".zip", StringComparison.OrdinalIgnoreCase ) )
            {
               var zf = new ZipFile( fullFileName );
               try
               {
                  foreach( var entry in zf )
                  {
                     if( entry is ZipEntry zipEntry )
                     {
                        if( zipEntry.IsFile && zipEntry.Name.EndsWith( ".png", StringComparison.OrdinalIgnoreCase ) )
                        {
                           var source = new ZipFileTranslatedImageSource( zf, zipEntry );
                           RegisterImageFromStream( fullFileName + Path.DirectorySeparatorChar + zipEntry.Name, source );
                        }
                     }
                  }
               }
               finally
               {
                  if( Settings.CacheTexturesInMemory )
                  {
                     zf.Close();
                  }
               }
            }
            else
            {
               var source = new FileSystemTranslatedImageSource( fullFileName );
               RegisterImageFromStream( fullFileName, source );
            }
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

                  XuaLogger.AutoTranslator.Warn( $"Replaced old file with name '{name}' registered with key old '{key}'." );
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, $"An error occurred while trying to rename file with key '{key}'." );
         }
      }

      internal void RegisterImageFromData( string textureName, string key, byte[] data )
      {
         var name = textureName.SanitizeForFileSystem();
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

         var fullName = Path.Combine( Settings.TexturesPath, fileName );
         File.WriteAllBytes( fullName, data );
         XuaLogger.AutoTranslator.Info( "Dumped texture file: " + fileName );

         _keyToFileName[ key ] = fullName;

         if( Settings.LoadUnmodifiedTextures )
         {
            var source = new FileSystemTranslatedImageSource( fullName );
            RegisterTranslatedImage( fullName, key, data, source );
         }
         else
         {
            RegisterUntranslatedImage( key );
         }
      }

      private void RegisterTranslatedImage( string fileName, string key, byte[] data, ITranslatedImageSource source )
      {
         _translatedImages[ key ] = new TranslatedImage( fileName, data, Settings.CacheTexturesInMemory ? null : source );
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
         if( _translatedImages.TryGetValue( key, out var translatedImage ) )
         {
            try
            {
               data = translatedImage.GetData();

               return data != null;
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurrd while attempting to load image: " + translatedImage.FileName );

               // clear the image???
               _translatedImages.Remove( key );
            }
         }

         data = null;
         return false;
      }
   }
}
