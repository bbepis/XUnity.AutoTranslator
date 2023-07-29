using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Shims;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   /// <summary>
   /// Helpers that ensures strings only contains standard ASCII characters.
   /// </summary>
   internal static class RomanizationHelper
   {
      public static string PostProcess( string text, TextPostProcessing postProcessing )
      {
         if( ( postProcessing & TextPostProcessing.ReplaceHtmlEntities ) != 0 )
         {
            text = WebUtility.HtmlDecode( text );
         }
         if( ( postProcessing & TextPostProcessing.ReplaceMacronWithCircumflex ) != 0 )
         {
            text = ConvertMacronToCircumflex( text );
         }
         if( ( postProcessing & TextPostProcessing.RemoveAllDiacritics ) != 0 )
         {
            text = DiacriticHelper.RemoveAllDiacritics( text );
         }
         if( ( postProcessing & TextPostProcessing.RemoveApostrophes ) != 0 )
         {
            text = RemoveNApostrophe( text );
         }
         if( ( postProcessing & TextPostProcessing.ReplaceWideCharacters ) != 0 )
         {
            text = ReplaceWideCharacters( text );
         }
         return text;
      }

      public static string ReplaceWideCharacters( string input )
      {
         var builder = new StringBuilder( input );
         var len = input.Length;
         bool wasChanged = false;

         for( int i = 0; i < len; i++ )
         {
            var c = builder[ i ];
            if( c >= 0xFF00 && c <= 0xFF5E )
            {
               wasChanged = true;
               builder[ i ] = (char)( c - 0xFEE0 );
            }

            if( c == 0x3000 )
            {
               wasChanged = true;
               builder[ i ] = (char)0x20;
            }
         }

         return wasChanged ? builder.ToString() : input;
      }

      public static string ConvertMacronToCircumflex( string romanizedJapaneseText )
      {
         var builder = new StringBuilder( romanizedJapaneseText.Length );
         for( int i = 0; i < romanizedJapaneseText.Length; i++ )
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
   }
}
