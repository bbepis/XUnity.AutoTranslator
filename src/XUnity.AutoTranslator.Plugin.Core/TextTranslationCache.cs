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
   class TextTranslationCache
   {
      private static readonly char[][] TranslationSplitters = new char[][] { new char[] { '\t' }, new char[] { '=' } };

      /// <summary>
      /// All the translations are stored in this dictionary.
      /// </summary>
      private Dictionary<string, string> _staticTranslations = new Dictionary<string, string>();
      private Dictionary<string, string> _translations = new Dictionary<string, string>();
      private Dictionary<string, string> _reverseTranslations = new Dictionary<string, string>();

      /// <summary>
      /// These are the new translations that has not yet been persisted to the file system.
      /// </summary>
      private object _writeToFileSync = new object();
      private Dictionary<string, string> _newTranslations = new Dictionary<string, string>();

      public TextTranslationCache()
      {
         LoadStaticTranslations();
      }

      private static IEnumerable<string> GetTranslationFiles()
      {
         return Directory.GetFiles( Path.Combine( PluginEnvironment.Current.DataPath, Settings.TranslationDirectory ).Parameterize(), $"*.txt", SearchOption.AllDirectories )
            .Select( x => x.Replace( "/", "\\" ) );
      }

      internal void LoadTranslationFiles()
      {
         try
         {
            var startTime = Time.realtimeSinceStartup;
            lock( _writeToFileSync )
            {
               Directory.CreateDirectory( Path.Combine( PluginEnvironment.Current.DataPath, Settings.TranslationDirectory ).Parameterize() );
               Directory.CreateDirectory( Path.GetDirectoryName( Settings.AutoTranslationsFilePath ) );

               var mainTranslationFile = Settings.AutoTranslationsFilePath;
               LoadTranslationsInFile( mainTranslationFile );
               foreach( var fullFileName in GetTranslationFiles().Reverse().Except( new[] { mainTranslationFile } ) )
               {
                  LoadTranslationsInFile( fullFileName );
               }
            }
            var endTime = Time.realtimeSinceStartup;
            XuaLogger.Current.Info( $"Loaded text files (took {Math.Round( endTime - startTime, 2 )} seconds)" );
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while loading translations." );
         }
      }

      private void LoadTranslationsInFile( string fullFileName )
      {
         if( File.Exists( fullFileName ) )
         {
            XuaLogger.Current.Debug( $"Loading texts: {fullFileName}." );

            string[] translations = File.ReadAllLines( fullFileName, Encoding.UTF8 );
            foreach( string translation in translations )
            {
               for( int i = 0; i < TranslationSplitters.Length; i++ )
               {
                  var splitter = TranslationSplitters[ i ];
                  string[] kvp = translation.Split( splitter, StringSplitOptions.None );
                  if( kvp.Length == 2 )
                  {
                     string key = TextHelper.Decode( kvp[ 0 ].TrimIfConfigured() );
                     string value = TextHelper.Decode( kvp[ 1 ] );

                     if( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( value ) && IsTranslatable( key ) )
                     {
                        AddTranslation( key, value );
                        break;
                     }
                  }
               }
            }
         }
      }

      private void LoadStaticTranslations()
      {
         if( Settings.UseStaticTranslations && Settings.FromLanguage == Settings.DefaultFromLanguage && Settings.Language == Settings.DefaultLanguage )
         {
            var tab = new char[] { '\t' };
            var equals = new char[] { '=' };
            var splitters = new char[][] { tab, equals };

            // load static translations from previous titles
            string[] translations = Properties.Resources.StaticTranslations.Split( new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );
            foreach( string translation in translations )
            {
               for( int i = 0; i < splitters.Length; i++ )
               {
                  var splitter = splitters[ i ];
                  string[] kvp = translation.Split( splitter, StringSplitOptions.None );
                  if( kvp.Length >= 2 )
                  {
                     string key = TextHelper.Decode( kvp[ 0 ].TrimIfConfigured() );
                     string value = TextHelper.Decode( kvp[ 1 ].TrimIfConfigured() );

                     if( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( value ) )
                     {
                        _staticTranslations[ key ] = value;
                        break;
                     }
                  }
               }
            }
         }
      }

      internal void SaveNewTranslationsToDisk()
      {
         if( _newTranslations.Count > 0 )
         {
            lock( _writeToFileSync )
            {
               if( _newTranslations.Count > 0 )
               {
                  using( var stream = File.Open( Settings.AutoTranslationsFilePath, FileMode.Append, FileAccess.Write ) )
                  using( var writer = new StreamWriter( stream, Encoding.UTF8 ) )
                  {
                     foreach( var kvp in _newTranslations )
                     {
                        writer.WriteLine( TextHelper.Encode( kvp.Key ) + '=' + TextHelper.Encode( kvp.Value ) );
                     }
                     writer.Flush();
                  }
                  _newTranslations.Clear();
               }
            }
         }
      }

      private bool HasTranslated( string key )
      {
         return _translations.ContainsKey( key );
      }

      private bool IsTranslation( string translation )
      {
         return _reverseTranslations.ContainsKey( translation );
      }

      private void AddTranslation( string key, string value )
      {
         _translations[ key ] = value;
         _reverseTranslations[ value ] = key;
      }

      private void QueueNewTranslationForDisk( string key, string value )
      {
         lock( _writeToFileSync )
         {
            _newTranslations[ key ] = value;
         }
      }

      internal void AddTranslationToCache( TranslationKey key, string value )
      {
         AddTranslation( key.GetDictionaryLookupKey(), value );
      }

      internal void AddTranslationToCache( string key, string value )
      {
         if( !HasTranslated( key ) )
         {
            AddTranslation( key, value );
            QueueNewTranslationForDisk( key, value );
         }
      }

      internal bool TryGetTranslation( TranslationKey key, out string value )
      {
         return TryGetTranslation( key.GetDictionaryLookupKey(), out value );
      }

      internal bool TryGetTranslation( string key, out string value )
      {
         var result = _translations.TryGetValue( key, out value );
         if( result )
         {
            return result;
         }
         else if( _staticTranslations.Count > 0 )
         {
            if( _staticTranslations.TryGetValue( key, out value ) )
            {
               QueueNewTranslationForDisk( key, value );
               AddTranslation( key, value );
               return true;
            }
         }
         return result;
      }

      internal bool TryGetReverseTranslation( string value, out string key )
      {
         return _reverseTranslations.TryGetValue( value, out key );
      }

      internal bool IsTranslatable( string str )
      {
         return LanguageHelper.ContainsLanguageSymbolsForSourceLanguage( str )
            //&& str.Length <= Settings.MaxCharactersPerTranslation
            && !IsTranslation( str )
            && !Settings.IgnoreTextStartingWith.Any( x => str.StartsWithStrict( x ) );
      }
   }
}
