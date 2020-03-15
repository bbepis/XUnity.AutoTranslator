using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UIResizeCache
   {
      private static readonly char[] Splitters = new char[] { '=' };

      private UIResizeAttachment _root = new UIResizeAttachment();

      public bool HasAnyResizeCommands { get; private set; } = false;

      private static IEnumerable<string> GetTranslationFiles()
      {
         return Directory.GetFiles( Settings.TranslationsPath, $"*.*", SearchOption.AllDirectories )
            .Where( x => x.EndsWith( "resizer.txt", StringComparison.OrdinalIgnoreCase ) || x.EndsWith( ".zip", StringComparison.OrdinalIgnoreCase ) );
      }

      internal void LoadResizeCommandsInFiles()
      {
         try
         {
            Directory.CreateDirectory( Settings.TranslationsPath );
            Directory.CreateDirectory( Path.GetDirectoryName( Settings.AutoTranslationsFilePath ) );

            _root = new UIResizeAttachment();

            foreach( var fullFileName in GetTranslationFiles().Reverse() )
            {
               LoadResizeCommandsInFile( fullFileName );
            }

            _root.Trim();

            XuaLogger.AutoTranslator.Debug( $"Loaded resize command text files." );

         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while resize commands." );
         }
      }

      private void LoadResizeCommandsInStream( Stream stream, string fullFileName )
      {
         if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Debug( $"Loading resize commands: {fullFileName}." );

         var reader = new StreamReader( stream, Encoding.UTF8 );
         {
            var context = new TranslationFileLoadingContext();

            string[] translations = reader.ReadToEnd().Split( new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
            foreach( string translatioOrDirective in translations )
            {
               if( Settings.EnableTranslationScoping )
               {
                  var directive = TranslationFileDirective.Create( translatioOrDirective );
                  if( directive != null )
                  {
                     context.Apply( directive );

                     if( !Settings.EnableSilentMode ) XuaLogger.AutoTranslator.Debug( "Directive in file: " + fullFileName + ": " + directive.ToString() );
                     continue;
                  }
               }

               if( context.IsExecutable( Settings.ApplicationName ) )
               {
                  string[] kvp = translatioOrDirective.Split( Splitters, StringSplitOptions.None );
                  if( kvp.Length == 2 )
                  {
                     string key = TextHelper.Decode( kvp[ 0 ] );
                     string value = TextHelper.Decode( kvp[ 1 ] );

                     if( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( value ) )
                     {
                        var levels = context.GetLevels();
                        if( levels.Count == 0 )
                        {
                           AddTranslation( key, value, TranslationScopes.None );
                        }
                        else
                        {
                           foreach( var level in levels )
                           {
                              AddTranslation( key, value, level );
                           }
                        }
                     }
                  }
               }
            }
         }
      }

      private void LoadResizeCommandsInFile( string fullFileName )
      {
         var fileExists = File.Exists( fullFileName );
         if( fileExists )
         {
            using( var stream = File.OpenRead( fullFileName ) )
            {
               // Perhaps use this instead???? https://github.com/icsharpcode/SharpZipLib/wiki/Unpack-a-zip-using-ZipInputStream
               if( fullFileName.EndsWith( ".zip", StringComparison.OrdinalIgnoreCase ) )
               {
                  using( var zipInputStream = new ZipInputStream( stream ) )
                  {
                     while( zipInputStream.GetNextEntry() is ZipEntry entry )
                     {
                        if( entry.IsFile && entry.Name.EndsWith( "resizer.txt", StringComparison.OrdinalIgnoreCase ) )
                        {
                           LoadResizeCommandsInStream( zipInputStream, fullFileName + Path.DirectorySeparatorChar + entry.Name );
                        }
                     }
                  }
               }
               else
               {
                  LoadResizeCommandsInStream( stream, fullFileName );
               }
            }
         }
      }

      private void AddTranslation( string key, string value, int scope )
      {
         if( key != null && value != null )
         {
            var ok = _root.AddResizeCommand( key, value, scope );

            HasAnyResizeCommands = ok || HasAnyResizeCommands;
         }
      }

      public bool TryGetUIResize( string[] paths, int scope, out UIResizeResult result )
      {
         return _root.TryGetUIResize( paths, scope, out result );
      }
   }
}
