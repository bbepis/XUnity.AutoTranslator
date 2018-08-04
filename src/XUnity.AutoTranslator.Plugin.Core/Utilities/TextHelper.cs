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
         { "ja-JP", ContainsJapaneseSymbols },
      };

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
               || ( c >= '\u3400' && c <= '\u4db5' ) ) // CJK unified ideographs Extension A - Rare kanji ( 3400 - 4dbf)
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
         // Remove these in newer version
         text = text.Replace( "\\r", "\r" );
         text = text.Replace( "\\n", "\n" );
         return text;
      }

      /// <summary>
      /// Encodes a text into a single-line serializable format.
      /// 
      /// Shamelessly stolen from original translation plugin.
      /// </summary>
      public static string Encode( string text )
      {
         text = text.Replace( "\r", "\\r" );
         text = text.Replace( "\n", "\\n" );
         return text;
      }
   }
}
