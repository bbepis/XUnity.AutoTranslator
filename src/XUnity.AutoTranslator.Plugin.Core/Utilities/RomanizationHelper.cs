using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   /// <summary>
   /// Helpers that ensures strings only contains standard ASCII characters.
   /// </summary>
   internal static class RomanizationHelper
   {
      public static string PostProcess( string text, RomajiPostProcessing postProcessing )
      {
         if( ( postProcessing & RomajiPostProcessing.ReplaceMacronWithCircumflex ) != 0 )
         {
            text = ConvertMacronToCircumflex( text );
         }
         if( ( postProcessing & RomajiPostProcessing.RemoveAllDiacritics ) != 0 )
         {
            text = RemoveAllDiacritics( text );
         }
         if( ( postProcessing & RomajiPostProcessing.ReplaceMacronWithCircumflex ) != 0 )
         {
            text = RemoveNApostrophe( text );
         }
         return text;
      }

      public static string ConvertMacronToCircumflex( string romanizedJapaneseText )
      {
         var builder = new StringBuilder( romanizedJapaneseText.Length );
         for( int i = 0 ; i < romanizedJapaneseText.Length ; i++ )
         {
            var c = romanizedJapaneseText[ i ];

            switch( c )
            {
               case 'Ā':
                  builder.Append( 'Â' );
                  break;
               case 'ā':
                  builder.Append( 'â' );
                  break;
               case 'Ī':
                  builder.Append( 'Î' );
                  break;
               case 'ī':
                  builder.Append( 'î' );
                  break;
               case 'Ū':
                  builder.Append( 'Û' );
                  break;
               case 'ū':
                  builder.Append( 'û' );
                  break;
               case 'Ē':
                  builder.Append( 'Ê' );
                  break;
               case 'ē':
                  builder.Append( 'ê' );
                  break;
               case 'Ō':
                  builder.Append( 'Ô' );
                  break;
               case 'ō':
                  builder.Append( 'ô' );
                  break;
               default:
                  builder.Append( c );
                  break;
            }
         }
         return builder.ToString();
      }

      public static string RemoveNApostrophe( string romanizedJapaneseText )
      {
         return romanizedJapaneseText
            .Replace( "n'", "n" )
            .Replace( "n’", "n" );
      }

      /// <summary>
      /// Remove diacritics (accents) from a string (for example ć -> c)
      /// </summary>
      /// <returns>ASCII compliant string</returns>
      public static string RemoveAllDiacritics( this string input )
      {
         var text = input.SafeNormalize( NormalizationForm.FormD );
         var chars = text.Where( c => CharUnicodeInfo.GetUnicodeCategory( c ) != UnicodeCategory.NonSpacingMark ).ToArray();
         return new string( chars ).SafeNormalize( NormalizationForm.FormC );
      }

      /// <summary>
      /// Safe version of normalize that doesn't crash on invalid code points in string.
      /// Instead the points are replaced with question marks.
      /// </summary>
      private static string SafeNormalize( this string input, NormalizationForm normalizationForm = NormalizationForm.FormC )
      {
         return ReplaceNonCharacters( input, '?' ).Normalize( normalizationForm );
      }

      private static string ReplaceNonCharacters( string input, char replacement )
      {
         var sb = new StringBuilder( input.Length );
         for( var i = 0 ; i < input.Length ; i++ )
         {
            if( char.IsSurrogatePair( input, i ) )
            {
               int c = char.ConvertToUtf32( input, i );
               i++;
               if( IsValidCodePoint( c ) )
                  sb.Append( char.ConvertFromUtf32( c ) );
               else
                  sb.Append( replacement );
            }
            else
            {
               char c = input[ i ];
               if( IsValidCodePoint( c ) )
                  sb.Append( c );
               else
                  sb.Append( replacement );
            }
         }
         return sb.ToString();
      }

      private static bool IsValidCodePoint( int point )
      {
         return point < 0xfdd0 || point >= 0xfdf0 && ( point & 0xffff ) != 0xffff && ( point & 0xfffe ) != 0xfffe && point <= 0x10ffff;
      }
   }
}
