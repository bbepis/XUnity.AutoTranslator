using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Utilities;
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
         return Directory.GetFiles( Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TranslationDirectory ).Parameterize(), $"*.resizer.txt", SearchOption.AllDirectories )
            .Select( x => x.Replace( "/", "\\" ) );
      }

      internal void LoadResizeCommandsInFiles()
      {
         try
         {
            Directory.CreateDirectory( Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TranslationDirectory ).Parameterize() );
            Directory.CreateDirectory( Path.GetDirectoryName( Settings.AutoTranslationsFilePath ) );

            _root = new UIResizeAttachment();

            foreach( var fullFileName in GetTranslationFiles().Reverse() )
            {
               LoadResizeCommandsInFile( fullFileName );
            }

            XuaLogger.AutoTranslator.Info( $"Loaded resize command text files." );

         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while resize commands." );
         }
      }

      private void LoadResizeCommandsInFile( string fullFileName )
      {
         var fileExists = File.Exists( fullFileName );
         if( fileExists )
         {
            XuaLogger.AutoTranslator.Debug( $"Loading resize commands: {fullFileName}." );

            var context = new TranslationFileLoadingContext();

            string[] translations = File.ReadAllLines( fullFileName, Encoding.UTF8 );
            foreach( string translatioOrDirective in translations )
            {
               if( Settings.EnableTranslationScoping )
               {
                  var directive = TranslationFileDirective.Create( translatioOrDirective );
                  if( directive != null )
                  {
                     context.Apply( directive );

                     XuaLogger.AutoTranslator.Debug( "Directive in file: " + fullFileName + ": " + directive.ToString() );
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
