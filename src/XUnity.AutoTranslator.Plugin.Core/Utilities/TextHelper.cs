using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   public static class TextHelper
   {
      private static readonly Dictionary<string, Func<string, bool>> LanguageSymbolChecks = new Dictionary<string, Func<string, bool>>( StringComparer.OrdinalIgnoreCase )
      {
         { "ja", ContainsJapaneseSymbols },
         { "ru", ContainsRussianSymbols },
         { "zh-CN", ContainsChineseSymbols },
         { "zh-TW", ContainsChineseSymbols },
         { "ko", ContainsKoreanSymbols },
         { "en", ContainsStandardLatinSymbols },
      };

      private static readonly HashSet<string> WhitespaceLanguages = new HashSet<string>
      {
         "ru", "ko", "en"
      };

      public static bool IsFromLanguageSupported( string code )
      {
         return LanguageSymbolChecks.ContainsKey( code );
      }

      public static bool RequiresWhitespaceUponLineMerging( string code )
      {
         return WhitespaceLanguages.Contains( code );
      }

      public static Func<string, bool> GetSymbolCheck( string language )
      {
         if( LanguageSymbolChecks.TryGetValue( language, out Func<string, bool> check ) )
         {
            return check;
         }
         return text => true;
      }

      public static bool ContainsJapaneseSymbols( string text )
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

      public static bool ContainsKoreanSymbols( string text )
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

      public static bool ContainsChineseSymbols( string text )
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

      public static bool ContainsRussianSymbols( string text )
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

      public static bool ContainsStandardLatinSymbols( string text )
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

      /// <summary>
      /// Decodes a text from a single-line serializable format.
      /// 
      /// Shamelessly stolen from original translation plugin.
      /// </summary>
      public static string Decode( string text )
      {
         return text.Replace( "\\r", "\r" )
            .Replace( "\\n", "\n" )
            .Replace( "%3D", "=" );
      }

      /// <summary>
      /// Encodes a text into a single-line serializable format.
      /// 
      /// Shamelessly stolen from original translation plugin.
      /// </summary>
      public static string Encode( string text )
      {
         return text.Replace( "\r", "\\r" )
            .Replace( "\n", "\\n" )
            .Replace( "=", "%3D" );
      }
   }
}
