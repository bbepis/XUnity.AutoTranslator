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

      private List<RegexTranslation> _defaultRegexes = new List<RegexTranslation>();
      private HashSet<string> _registeredRegexes = new HashSet<string>();

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
         return Directory.GetFiles( Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TranslationDirectory ).Parameterize(), $"*.txt", SearchOption.AllDirectories )
            .Select( x => x.Replace( "/", "\\" ) );
      }

      internal void LoadTranslationFiles()
      {
         try
         {
            var startTime = Time.realtimeSinceStartup;
            lock( _writeToFileSync )
            {
               Directory.CreateDirectory( Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TranslationDirectory ).Parameterize() );
               Directory.CreateDirectory( Path.GetDirectoryName( Settings.AutoTranslationsFilePath ) );

               _registeredRegexes.Clear();
               _defaultRegexes.Clear();
               _translations.Clear();
               _reverseTranslations.Clear();
               Settings.Replacements.Clear();

               var mainTranslationFile = Settings.AutoTranslationsFilePath;
               var substitutionFile = Settings.SubstitutionFilePath;
               LoadTranslationsInFile( mainTranslationFile, false );
               LoadTranslationsInFile( substitutionFile, true );
               foreach( var fullFileName in GetTranslationFiles().Reverse().Except( new[] { mainTranslationFile } ) )
               {
                  LoadTranslationsInFile( fullFileName, false );
               }
            }
            var endTime = Time.realtimeSinceStartup;
            XuaLogger.Current.Info( $"Loaded text files ({_translations.Count} translations and {_defaultRegexes.Count} regex translations) (took {Math.Round( endTime - startTime, 2 )} seconds)" );
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while loading translations." );
         }
      }

      private void LoadTranslationsInFile( string fullFileName, bool isSubstitutionFile )
      {
         var fileExists = File.Exists( fullFileName );
         if( fileExists || isSubstitutionFile )
         {
            if( fileExists )
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
                        string key = TextHelper.Decode( kvp[ 0 ] );
                        string value = TextHelper.Decode( kvp[ 1 ] );

                        if( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( value ) && IsTranslatable( key ) )
                        {
                           if( isSubstitutionFile )
                           {
                              if( key != null && value != null )
                              {
                                 Settings.Replacements[ key ] = value;
                              }
                           }
                           else
                           {
                              if( key.StartsWith( "r:" ) )
                              {
                                 try
                                 {
                                    var regex = new RegexTranslation( key, value );

                                    AddTranslationRegex( regex );
                                 }
                                 catch( Exception e )
                                 {
                                    XuaLogger.Current.Warn( e, $"An error occurred while constructing regex translation: '{translation}'." );
                                 }
                              }
                              else
                              {
                                 AddTranslation( key, value );

                                 // also add a modified version of the translation
                                 var ukey = new UntranslatedText( key, false, false );
                                 var uvalue = new UntranslatedText( value, false, false );
                                 if( ukey.TrimmedTranslatableText != key )
                                 {
                                    AddTranslation( ukey.TrimmedTranslatableText, uvalue.TrimmedTranslatableText );
                                 }
                              }
                              break;
                           }
                        }
                     }
                  }
               }
            }
            else if( isSubstitutionFile )
            {
               using( var stream = File.Create( fullFileName ) )
               {
                  stream.Write( new byte[] { 0xEF, 0xBB, 0xBF }, 0, 3 ); // UTF-8 BOM
                  stream.Close();
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
                     string key = TextHelper.Decode( kvp[ 0 ] );
                     string value = TextHelper.Decode( kvp[ 1 ] );

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

      private void AddTranslationRegex( RegexTranslation regex )
      {
         if( !_registeredRegexes.Contains( regex.Original ) )
         {
            _registeredRegexes.Add( regex.Original );
            _defaultRegexes.Add( regex );
         }
         //else
         //{
         //   XuaLogger.Current.Warn( $"Could not register translation regex '{regex.Original}' because it has already been registered." );
         //}
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
         if( key != null && value != null )
         {
            _translations[ key ] = value;
            _reverseTranslations[ value ] = key;
         }
      }

      private void QueueNewTranslationForDisk( string key, string value )
      {
         lock( _writeToFileSync )
         {
            _newTranslations[ key ] = value;
         }
      }

      internal void AddTranslationToCache( string key, string value, bool persistToDisk = true )
      {
         if( !HasTranslated( key ) )
         {
            AddTranslation( key, value );

            // also add a trimmed version of the translation
            var ukey = new UntranslatedText( key, false, false );
            var uvalue = new UntranslatedText( value, false, false );
            if( ukey.TrimmedTranslatableText != key && !HasTranslated( ukey.TrimmedTranslatableText ) )
            {
               AddTranslation( ukey.TrimmedTranslatableText, uvalue.TrimmedTranslatableText );
            }

            if( persistToDisk )
            {
               QueueNewTranslationForDisk( key, value );
            }
         }
      }

      internal bool TryGetTranslation( UntranslatedText key, bool allowRegex, out string value )
      {
         var translatableText = key.TranslatableText;
         var result = _translations.TryGetValue( translatableText, out value );
         if( result )
         {
            return result;
         }

         var trimmedTranslatableText = key.TrimmedTranslatableText;
         if( trimmedTranslatableText != translatableText )
         {
            result = _translations.TryGetValue( trimmedTranslatableText, out value );
            if( result )
            {
               // add an unmodifiedKey to the dictionary
               var unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;

               XuaLogger.Current.Info( $"Whitespace difference: '{key.TrimmedTranslatableText}' => '{value}'" );
               AddTranslationToCache( translatableText, unmodifiedValue, Settings.CacheWhitespaceDifferences );

               value = unmodifiedValue;
               return result;
            }
         }

         if( allowRegex )
         {
            bool found = false;

            for( int i = _defaultRegexes.Count - 1; i > -1; i-- )
            {
               var regex = _defaultRegexes[ i ];
               try
               {
                  var match = regex.CompiledRegex.Match( translatableText );
                  if( !match.Success ) continue;

                  var translation = regex.CompiledRegex.Replace( translatableText, regex.Translation );

                  AddTranslationToCache( translatableText, translation, Settings.CacheRegexLookups ); // Would store it to file... Should we????

                  value = translation;
                  found = true;

                  XuaLogger.Current.Info( $"Regex lookup: '{key.TrimmedTranslatableText}' => '{value}'" );
                  break;
               }
               catch( Exception e )
               {
                  _defaultRegexes.RemoveAt( i );

                  XuaLogger.Current.Error( e, $"Failed while attempting to replace or match text of regex '{regex.Original}'. Removing that regex from the cache." );
               }
            }

            if( found )
            {
               return true;
            }
         }

         if( _staticTranslations.Count > 0 )
         {
            if( _staticTranslations.TryGetValue( translatableText, out value ) )
            {
               AddTranslationToCache( translatableText, value );
               return true;
            }
            else if( _staticTranslations.TryGetValue( trimmedTranslatableText, out value ) )
            {
               var unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;
               AddTranslationToCache( translatableText, unmodifiedValue );

               value = unmodifiedValue;
               return true;
            }
         }

         return result;
      }

      internal bool TryGetReverseTranslation( string value, out string key )
      {
         return _reverseTranslations.TryGetValue( value, out key );
      }

      internal bool IsTranslatable( string text )
      {
         return LanguageHelper.IsTranslatable( text ) && !IsTranslation( text );
      }
   }
}
