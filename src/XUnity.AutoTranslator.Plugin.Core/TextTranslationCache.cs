using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;
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
      private Dictionary<string, string> _tokenTranslations = new Dictionary<string, string>();
      private Dictionary<string, string> _reverseTokenTranslations = new Dictionary<string, string>();
      private HashSet<string> _partialTranslations = new HashSet<string>();

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
               _partialTranslations.Clear();
               _tokenTranslations.Clear();
               _reverseTokenTranslations.Clear();
               Settings.Replacements.Clear();

               var mainTranslationFile = Settings.AutoTranslationsFilePath;
               var substitutionFile = Settings.SubstitutionFilePath;
               LoadTranslationsInFile( mainTranslationFile, false );
               LoadTranslationsInFile( substitutionFile, true );
               foreach( var fullFileName in GetTranslationFiles().Reverse().Except( new[] { mainTranslationFile, substitutionFile } ) )
               {
                  LoadTranslationsInFile( fullFileName, false );
               }
            }
            var endTime = Time.realtimeSinceStartup;
            XuaLogger.Current.Info( $"Loaded text files ({_translations.Count} translations and {_defaultRegexes.Count} regex translations) (took {Math.Round( endTime - startTime, 2 )} seconds)" );

            // generate variations of created translations
            {
               startTime = Time.realtimeSinceStartup;

               foreach( var kvp in _translations.ToList() )
               {
                  // also add a modified version of the translation
                  var ukey = new UntranslatedText( kvp.Key, false, true, Settings.FromLanguageUsesWhitespaceBetweenWords );
                  var uvalue = new UntranslatedText( kvp.Value, false, true, Settings.ToLanguageUsesWhitespaceBetweenWords );
                  if( ukey.Original_Text_ExternallyTrimmed != kvp.Key && !HasTranslated( ukey.Original_Text_ExternallyTrimmed ) )
                  {
                     AddTranslation( ukey.Original_Text_ExternallyTrimmed, uvalue.Original_Text_ExternallyTrimmed );
                  }
                  if( ukey.Original_Text_ExternallyTrimmed != ukey.Original_Text_FullyTrimmed && !HasTranslated( ukey.Original_Text_FullyTrimmed ) )
                  {
                     AddTranslation( ukey.Original_Text_FullyTrimmed, uvalue.Original_Text_FullyTrimmed );
                  }
               }

               XuaLogger.Current.Info( $"Created variation translations (took {Math.Round( endTime - startTime, 2 )} seconds)" );
               endTime = Time.realtimeSinceStartup;
            }



            // generate token translations, which are online allowed when getting 'token' translations
            if( Settings.GeneratePartialTranslations )
            {
               startTime = Time.realtimeSinceStartup;

               foreach( var kvp in _translations.ToList() )
               {
                  CreatePartialTranslationsFor( kvp.Key, kvp.Value );
               }

               XuaLogger.Current.Info( $"Created partial translations (took {Math.Round( endTime - startTime, 2 )} seconds)" );
               endTime = Time.realtimeSinceStartup;
            }

            var parser = new RichTextParser();
            startTime = Time.realtimeSinceStartup;

            foreach( var kvp in _translations.ToList() )
            {
               var untranslatedResult = parser.Parse( kvp.Key );
               if( untranslatedResult.Succeeded )
               {
                  var translatedResult = parser.Parse( kvp.Value );
                  if( translatedResult.Succeeded && untranslatedResult.Arguments.Count == untranslatedResult.Arguments.Count )
                  {
                     foreach( var ukvp in untranslatedResult.Arguments )
                     {
                        var untranslatedToken = ukvp.Value;
                        if( translatedResult.Arguments.TryGetValue( ukvp.Key, out var translatedToken ) )
                        {
                           AddTokenTranslation( untranslatedToken, translatedToken );
                        }
                     }
                  }
               }
            }

            XuaLogger.Current.Info( $"Created token translations (took {Math.Round( endTime - startTime, 2 )} seconds)" );
            endTime = Time.realtimeSinceStartup;
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while loading translations." );
         }
      }

      private void CreatePartialTranslationsFor( string originalText, string translatedText )
      {
         // determine how many tokens in both strings
         var originalTextTokens = Tokenify( originalText );
         var translatedTextTokens = Tokenify( translatedText );

         double rate = translatedTextTokens.Count / (double)originalTextTokens.Count;

         int translatedTextCursor = 0;
         string originalTextPartial = string.Empty;
         string translatedTextPartial = string.Empty;

         for( int i = 0; i < originalTextTokens.Count; i++ )
         {
            var originalTextToken = originalTextTokens[ i ];
            if( originalTextToken.IsVariable )
            {
               originalTextPartial += "{{" + originalTextToken.Character + "}}";
            }
            else
            {
               originalTextPartial += originalTextToken.Character;
            }

            var length = (int)( Math.Round( ( i + 1 ) * rate, 0, MidpointRounding.AwayFromZero ) );
            for( int j = translatedTextCursor; j < length; j++ )
            {
               var translatedTextToken = translatedTextTokens[ j ];
               if( translatedTextToken.IsVariable )
               {
                  translatedTextPartial += "{{" + translatedTextToken.Character + "}}";
               }
               else
               {
                  translatedTextPartial += translatedTextToken.Character;
               }
            }

            translatedTextCursor = length;

            if( !HasTranslated( originalTextPartial ) )
            {
               AddTranslation( originalTextPartial, translatedTextPartial );
               _partialTranslations.Add( originalTextPartial );
            }
         }
      }

      private static List<TranslationCharacterToken> Tokenify( string text )
      {
         var result = new List<TranslationCharacterToken>( text.Length );

         for( int i = 0; i < text.Length; i++ )
         {
            var c = text[ i ];
            if( c == '{' && text.Length > i + 4 && text[ i + 1 ] == '{' && text[ i + 3 ] == '}' && text[ i + 4 ] == '}' )
            {
               c = text[ i + 2 ];
               result.Add( new TranslationCharacterToken( c, true ) );

               i += 4;
            }
            else
            {
               result.Add( new TranslationCharacterToken( c, false ) );
            }
         }

         return result;
      }

      private struct TranslationCharacterToken
      {
         public TranslationCharacterToken( char c, bool isVariable )
         {
            Character = c;
            IsVariable = isVariable;
         }

         public char Character { get; set; }

         public bool IsVariable { get; set; }
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

                        if( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( value ) )
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
            // load static translations from previous titles
            string[] translations = Properties.Resources.StaticTranslations.Split( new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );
            foreach( string translation in translations )
            {
               for( int i = 0; i < TranslationSplitters.Length; i++ )
               {
                  var splitter = TranslationSplitters[ i ];
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
         return !HasTranslated( translation ) && _reverseTranslations.ContainsKey( translation );
      }

      private bool IsTokenTranslation( string translation )
      {
         return _reverseTokenTranslations.ContainsKey( translation );
      }

      private void AddTranslation( string key, string value )
      {
         if( key != null && value != null )
         {
            _translations[ key ] = value;
            _reverseTranslations[ value ] = key;
         }
      }

      private void AddTokenTranslation( string key, string value )
      {
         if( key != null && value != null )
         {
            _tokenTranslations[ key ] = value;
            _reverseTokenTranslations[ value ] = key;
         }
      }

      private void QueueNewTranslationForDisk( string key, string value )
      {
         lock( _writeToFileSync )
         {
            _newTranslations[ key ] = value;
         }
      }

      internal void AddTranslationToCache( string key, string value, bool persistToDisk, TranslationType type )
      {
         if( ( type & TranslationType.Token ) == TranslationType.Token )
         {
            AddTokenTranslation( key, value );
         }

         if( ( type & TranslationType.Full ) == TranslationType.Full )
         {
            if( !HasTranslated( key ) )
            {
               AddTranslation( key, value );

               // also add a modified version of the translation
               var ukey = new UntranslatedText( key, false, true, Settings.FromLanguageUsesWhitespaceBetweenWords );
               var uvalue = new UntranslatedText( value, false, true, Settings.ToLanguageUsesWhitespaceBetweenWords );
               if( ukey.Original_Text_ExternallyTrimmed != key && !HasTranslated( ukey.Original_Text_ExternallyTrimmed ) )
               {
                  AddTranslation( ukey.Original_Text_ExternallyTrimmed, uvalue.Original_Text_ExternallyTrimmed );
               }
               if( ukey.Original_Text_ExternallyTrimmed != ukey.Original_Text_FullyTrimmed && !HasTranslated( ukey.Original_Text_FullyTrimmed ) )
               {
                  AddTranslation( ukey.Original_Text_FullyTrimmed, uvalue.Original_Text_FullyTrimmed );
               }

               if( persistToDisk )
               {
                  QueueNewTranslationForDisk( key, value );
               }
            }
         }
      }

      internal bool TryGetTranslation( UntranslatedText key, bool allowRegex, bool allowToken, out string value )
      {
         bool result;
         string untemplated;
         string unmodifiedValue;
         string unmodifiedKey;

         // first lookup token, if we allow it! - ONLY ORIGINAL and INTERNALLY FIXED VARIATIONS
         if( allowToken )
         {
            if( key.IsTemplated && !key.IsFromSpammingComponent )
            {
               // key.TemplatedOriginal_Text = '   What are you \ndoing here, {{A}}?'
               // untemplated                = '   What are you \ndoing here, Sophie?'

               // lookup original
               untemplated = key.Untemplate( key.TemplatedOriginal_Text );
               result = _tokenTranslations.TryGetValue( untemplated, out value );
               if( result )
               {
                  return result;
               }

               // lookup internally trimmed
               if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
               {
                  // key.TemplatedOriginal_Text_InternallyTrimmed = '   What are you doing here, {{A}}?'
                  // untemplated                                  = '   What are you doing here, Sophie?'

                  untemplated = key.Untemplate( key.TemplatedOriginal_Text_InternallyTrimmed );
                  result = _tokenTranslations.TryGetValue( untemplated, out value );
                  if( result )
                  {
                     return result;
                  }
               }
            }

            // lookup original

            // key.TemplatedOriginal_Text = '   What are you \ndoing here, {{A}}?'
            result = _tokenTranslations.TryGetValue( key.TemplatedOriginal_Text, out value );
            if( result )
            {
               return result;
            }

            // lookup internally trimmed
            if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_InternallyTrimmed = '   What are you doing here, {{A}}?'

               result = _tokenTranslations.TryGetValue( key.TemplatedOriginal_Text_InternallyTrimmed, out value );
               if( result )
               {
                  return result;
               }
            }
         }

         // lookup UNTEMPLATED translations - ALL VARIATIONS
         if( key.IsTemplated && !key.IsFromSpammingComponent )
         {
            // key.TemplatedOriginal_Text = '   What are you \ndoing here, {{A}}?'
            // untemplated                = '   What are you \ndoing here, Sophie?'

            // lookup original
            untemplated = key.Untemplate( key.TemplatedOriginal_Text );
            result = _translations.TryGetValue( untemplated, out value );
            if( result )
            {
               return result;
            }

            // lookup original minus external whitespace
            if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_ExternallyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_ExternallyTrimmed = 'What are you \ndoing here, {{A}}?'
               // untemplated                                  = 'What are you \ndoing here, Sophie?'

               untemplated = key.Untemplate( key.TemplatedOriginal_Text_ExternallyTrimmed );
               result = _translations.TryGetValue( untemplated, out value );
               if( result )
               {
                  // WHITESPACE DIFFERENCE, Store new value
                  unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;
                  unmodifiedKey = key.Untemplate( key.TemplatedOriginal_Text );

                  XuaLogger.Current.Info( $"Whitespace difference (c1): '{unmodifiedKey}' => '{unmodifiedValue}'" );
                  AddTranslationToCache( unmodifiedKey, unmodifiedValue, Settings.CacheWhitespaceDifferences, TranslationType.Full );

                  value = unmodifiedValue;
                  return result;
               }
            }

            // lookup internally trimmed
            if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_InternallyTrimmed = '   What are you doing here, {{A}}?'
               // untemplated                                  = '   What are you doing here, Sophie?'

               untemplated = key.Untemplate( key.TemplatedOriginal_Text_InternallyTrimmed );
               result = _translations.TryGetValue( untemplated, out value );
               if( result )
               {
                  // WHITESPACE DIFFERENCE, Store new value
                  unmodifiedValue = value;
                  unmodifiedKey = key.Untemplate( key.TemplatedOriginal_Text );

                  XuaLogger.Current.Info( $"Whitespace difference (c2): '{unmodifiedKey}' => '{unmodifiedValue}'" );
                  AddTranslationToCache( unmodifiedKey, unmodifiedValue, Settings.CacheWhitespaceDifferences, TranslationType.Full );

                  value = unmodifiedValue;
                  return result;
               }
            }

            // lookup internally trimmed minus external whitespace
            if( !ReferenceEquals( key.TemplatedOriginal_Text_InternallyTrimmed, key.TemplatedOriginal_Text_FullyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_FullyTrimmed = 'What are you doing here, {{A}}?'
               // untemplated                             = 'What are you doing here, Sophie?'

               untemplated = key.Untemplate( key.TemplatedOriginal_Text_FullyTrimmed );
               result = _translations.TryGetValue( untemplated, out value );
               if( result )
               {
                  // WHITESPACE DIFFERENCE, Store new value
                  unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;
                  unmodifiedKey = key.Untemplate( key.TemplatedOriginal_Text );

                  XuaLogger.Current.Info( $"Whitespace difference (c3): '{unmodifiedKey}' => '{unmodifiedValue}'" );
                  AddTranslationToCache( unmodifiedKey, unmodifiedValue, Settings.CacheWhitespaceDifferences, TranslationType.Full );

                  value = unmodifiedValue;
                  return result;
               }
            }
         }

         // lookup original - ALL VARATIONS

         // key.TemplatedOriginal_Text = '   What are you \ndoing here, {{A}}?'
         result = _translations.TryGetValue( key.TemplatedOriginal_Text, out value );
         if( result )
         {
            return result;
         }

         // lookup original minus external whitespace
         if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_ExternallyTrimmed ) )
         {
            // key.TemplatedOriginal_Text_ExternallyTrimmed = 'What are you \ndoing here, {{A}}?'

            result = _translations.TryGetValue( key.TemplatedOriginal_Text_ExternallyTrimmed, out value );
            if( result )
            {
               // WHITESPACE DIFFERENCE, Store new value
               unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;

               XuaLogger.Current.Info( $"Whitespace difference (c4): '{key.TemplatedOriginal_Text}' => '{unmodifiedValue}'" );
               AddTranslationToCache( key.TemplatedOriginal_Text, unmodifiedValue, Settings.CacheWhitespaceDifferences, TranslationType.Full );

               value = unmodifiedValue;
               return result;
            }
         }

         // lookup internally trimmed
         if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
         {
            // key.TemplatedOriginal_Text_InternallyTrimmed = '   What are you doing here, {{A}}?'

            result = _translations.TryGetValue( key.TemplatedOriginal_Text_InternallyTrimmed, out value );
            if( result )
            {
               // WHITESPACE DIFFERENCE, Store new value
               XuaLogger.Current.Info( $"Whitespace difference (c5): '{key.TemplatedOriginal_Text}' => '{value}'" );
               AddTranslationToCache( key.TemplatedOriginal_Text, value, Settings.CacheWhitespaceDifferences, TranslationType.Full ); // FIXED: using templated original

               return result;
            }
         }

         // lookup internally trimmed minus external whitespace
         if( !ReferenceEquals( key.TemplatedOriginal_Text_InternallyTrimmed, key.TemplatedOriginal_Text_FullyTrimmed ) )
         {
            // key.TemplatedOriginal_Text_FullyTrimmed = 'What are you doing here, {{A}}?'

            result = _translations.TryGetValue( key.TemplatedOriginal_Text_FullyTrimmed, out value );
            if( result )
            {
               // WHITESPACE DIFFERENCE, Store new value
               unmodifiedValue = key.LeadingWhitespace + value + key.TrailingWhitespace;

               XuaLogger.Current.Info( $"Whitespace difference (c6): '{key.TemplatedOriginal_Text}' => '{unmodifiedValue}'" );
               AddTranslationToCache( key.TemplatedOriginal_Text, unmodifiedValue, Settings.CacheWhitespaceDifferences, TranslationType.Full );

               value = unmodifiedValue;
               return result;
            }
         }

         // regex lookups - ONLY ORIGNAL VARIATION
         if( allowRegex )
         {
            for( int i = _defaultRegexes.Count - 1; i > -1; i-- )
            {
               var regex = _defaultRegexes[ i ];
               try
               {
                  var match = regex.CompiledRegex.Match( key.TemplatedOriginal_Text );
                  if( !match.Success ) continue;

                  value = regex.CompiledRegex.Replace( key.TemplatedOriginal_Text, regex.Translation );

                  XuaLogger.Current.Info( $"Regex lookup: '{key.TemplatedOriginal_Text}' => '{value}'" );
                  AddTranslationToCache( key.TemplatedOriginal_Text, value, Settings.CacheRegexLookups, TranslationType.Full );

                  return true;
               }
               catch( Exception e )
               {
                  _defaultRegexes.RemoveAt( i );

                  XuaLogger.Current.Error( e, $"Failed while attempting to replace or match text of regex '{regex.Original}'. Removing that regex from the cache." );
               }
            }
         }

         // static lookups - ALL VARIATIONS
         if( _staticTranslations.Count > 0 )
         {
            // lookup based on templated value alone, which may also be the original text
            // key.TemplatedOriginal_Text = '   What are you \ndoing here, {{A}}?'

            // lookup original
            result = _staticTranslations.TryGetValue( key.TemplatedOriginal_Text, out value );
            if( result )
            {
               XuaLogger.Current.Info( $"Static lookup: '{key.TemplatedOriginal_Text}' => '{value}'" );
               AddTranslationToCache( key.TemplatedOriginal_Text, value, true, TranslationType.Full );

               return result;
            }

            // lookup internally trimmed
            if( !ReferenceEquals( key.TemplatedOriginal_Text, key.TemplatedOriginal_Text_InternallyTrimmed ) )
            {
               // key.TemplatedOriginal_Text_InternallyTrimmed = '   What are you doing here, {{A}}?'

               result = _staticTranslations.TryGetValue( key.TemplatedOriginal_Text_InternallyTrimmed, out value );
               if( result )
               {
                  XuaLogger.Current.Info( $"Static lookup: '{key.TemplatedOriginal_Text_InternallyTrimmed}' => '{value}'" );
                  AddTranslationToCache( key.TemplatedOriginal_Text, value, true, TranslationType.Full );

                  return result;
               }
            }
         }

         return result;
      }

      internal bool TryGetReverseTranslation( string value, out string key )
      {
         return _reverseTranslations.TryGetValue( value, out key );
      }

      internal bool IsTranslatable( string text, bool isToken )
      {
         var translatable = LanguageHelper.IsTranslatable( text ) && !IsTranslation( text );
         if( isToken && translatable )
         {
            translatable = !IsTokenTranslation( text );
         }
         return translatable;
      }

      internal bool IsPartial( string text )
      {
         return _partialTranslations.Contains( text );
      }
   }
}
