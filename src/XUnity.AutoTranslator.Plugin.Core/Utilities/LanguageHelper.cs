using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   /// <summary>
   /// Class containing methods related to the loaded languages.
   /// </summary>
   public static class LanguageHelper
   {
      internal static bool HasRedirectedTexts = false;

      private const string MogolianVowelSeparatorString = "\u180e";
      private const char MogolianVowelSeparatorCharacter = '\u180e';

      private static Func<string, bool> DefaultSymbolCheck;
      private static readonly Dictionary<string, Func<string, bool>> LanguageSymbolChecks = new Dictionary<string, Func<string, bool>>( StringComparer.OrdinalIgnoreCase )
      {
         { "ja", ContainsJapaneseSymbols },
         { "ru", ContainsRussianSymbols },
         { "zh-CN", ContainsChineseSymbols },
         { "zh-TW", ContainsChineseSymbols },
         { "zh-Hans", ContainsChineseSymbols },
         { "zh-Hant", ContainsChineseSymbols },
         { "zh", ContainsChineseSymbols },
         { "ko", ContainsKoreanSymbols },
         { "en", ContainsStandardLatinSymbols },
         { "auto", text => true },
      };

      private static readonly HashSet<string> LanguagesNotUsingWhitespaceBetweenWords = new HashSet<string>
      {
         "ja", "zh", "zh-CN", "zh-TW", "zh-Hans", "zh-Hant"
      };

      internal static bool RequiresWhitespaceUponLineMerging( string code )
      {
         return !LanguagesNotUsingWhitespaceBetweenWords.Contains( code );
      }

      internal static Func<string, bool> GetSymbolCheck( string language )
      {
         if( LanguageSymbolChecks.TryGetValue( language, out Func<string, bool> check ) )
         {
            return check;
         }
         return text => true;
      }

      internal static bool ContainsLanguageSymbolsForSourceLanguage( string text )
      {
         if( DefaultSymbolCheck == null )
         {
            DefaultSymbolCheck = GetSymbolCheck( Settings.FromLanguage );
         }

         return DefaultSymbolCheck( text );
      }

      /// <summary>
      /// Checks if the text contains a variable.
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static bool ContainsVariableSymbols(string text)
      {
         var fidx = text.IndexOf( '{' );
         return fidx > -1
            && text.IndexOf( '}', fidx ) > fidx;
      }

      /// <summary>
      /// Check if the text has been redirected through the MakeRedirected method.
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static bool IsRedirected( this string text )
      {
         if( text.Length > 0 )
         {
            switch( Settings.RedirectedResourceDetectionStrategy )
            {
               case RedirectedResourceDetection.AppendMongolianVowelSeparator:
               case RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAppended:
               case RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAll:
                  return text.Contains( MogolianVowelSeparatorString ); // Using a char here causes Linq usage which is MASSIVELY slower, since String.Contains(char) does not exist in net35
               case RedirectedResourceDetection.None:
               default:
                  return false;
            }
         }
         return false;
      }

      /// <summary>
      /// This is the reverse of the MakeRedirected method. 
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static string FixRedirected( this string text )
      {
         switch( Settings.RedirectedResourceDetectionStrategy )
         {
            case RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAppended:
               if( text.Length > 0 && text[ text.Length - 1 ] == MogolianVowelSeparatorCharacter )
               {
                  return text.Substring( 0, text.Length - 1 );
               }
               break;
            case RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAll:
               return text.Replace( MogolianVowelSeparatorString, string.Empty );
            case RedirectedResourceDetection.AppendMongolianVowelSeparator:
            case RedirectedResourceDetection.None:
            default:
               return text;
         }

         return text;
      }

      /// <summary>
      /// Transforms the text in such a way that the plugin can recognize it when
      /// it is displayed in a UI component. This is to avoid translating the text.
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static string MakeRedirected( this string text )
      {
         switch( Settings.RedirectedResourceDetectionStrategy )
         {
            case RedirectedResourceDetection.AppendMongolianVowelSeparator:
            case RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAppended:
            case RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAll:
               if( ContainsLanguageSymbolsForSourceLanguage( text ) || ContainsVariableSymbols( text ) )
               {
                  HasRedirectedTexts = Settings.RedirectedResourceDetectionStrategy != RedirectedResourceDetection.None;

                  return text + MogolianVowelSeparatorCharacter;
               }
               break;
            case RedirectedResourceDetection.None:
            default:
               break;
         }
         return text;
      }

      /// <summary>
      /// Gets a bool indicating if the given string is translatable based on whether or not it
      /// contains symbols of the source language.
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static bool IsTranslatable( string text )
      {
         return ContainsLanguageSymbolsForSourceLanguage( text )
            && !Settings.IgnoreTextStartingWith.Any( x => text.StartsWithStrict( x ) );
      }

      internal static bool ContainsJapaneseSymbols( string text )
      {
         // Unicode Kanji Table:
         // http://www.rikai.com/library/kanjitables/kanji_codes.unicode.shtml
         foreach( var c in text )
         {
            if( ( c >= '\u3021' && c <= '\u3029' ) // kana-like symbols
               || ( c >= '\u3031' && c <= '\u3035' ) // kana-like symbols
               || ( c >= '\u3041' && c <= '\u3096' ) // hiragana
               || ( c >= '\u30a1' && c <= '\u30fa' ) // katakana
               || ( c >= '\uff66' && c <= '\uff9d' ) // half-width katakana
               || ( c >= '\u4e00' && c <= '\u9faf' ) // CJK unifed ideographs - Common and uncommon kanji
               || ( c >= '\u3400' && c <= '\u4dbf' ) // CJK unified ideographs Extension A - Rare kanji ( 3400 - 4dbf)
               || ( c >= '\uf900' && c <= '\ufaff' ) ) // CJK Compatibility Ideographs
            {
               return true;
            }
         }
         return false;
      }

      internal static bool ContainsKoreanSymbols( string text )
      {
         foreach( var c in text )
         {
            if( ( c >= '\uac00' && c <= '\ud7af' ) ) // Hangul Syllables
            {
               return true;
            }
         }
         return false;
      }

      internal static bool ContainsChineseSymbols( string text )
      {
         foreach( var c in text )
         {
            if( ( c >= '\u4e00' && c <= '\u9faf' )
               || ( c >= '\u3400' && c <= '\u4dbf' )
               || ( c >= '\uf900' && c <= '\ufaff' ) )
            {
               return true;
            }
         }
         return false;
      }

      internal static bool ContainsRussianSymbols( string text )
      {
         foreach( var c in text )
         {
            if( ( c >= '\u0400' && c <= '\u04ff' )
               || ( c >= '\u0500' && c <= '\u052f' )
               || ( c >= '\u2de0' && c <= '\u2dff' )
               || ( c >= '\ua640' && c <= '\ua69f' )
               || ( c >= '\u1c80' && c <= '\u1c88' )
               || ( c >= '\ufe2e' && c <= '\ufe2f' )
               || ( c == '\u1d2b' || c == '\u1d78' ) )
            {
               return true;
            }
         }
         return false;
      }

      internal static bool ContainsStandardLatinSymbols( string text )
      {
         foreach( var c in text )
         {
            if( ( c >= '\u0041' && c <= '\u005a' )
               || ( c >= '\u0061' && c <= '\u007a' ) )
            {
               return true;
            }
         }
         return false;
      }
   }
}
