using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class DiacriticHelper
   {
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
         for( var i = 0; i < input.Length; i++ )
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
