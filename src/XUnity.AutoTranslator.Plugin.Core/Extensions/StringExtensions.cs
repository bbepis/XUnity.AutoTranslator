using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class StringExtensions
   {
      private static readonly HashSet<char> Numbers = new HashSet<char>
      {
         '0',
         '1',
         '2',
         '3',
         '4',
         '5',
         '6',
         '7',
         '8',
         '9',
         '０',
         '１',
         '２',
         '３',
         '４',
         '５',
         '６',
         '７',
         '８',
         '９'
      };

      private static readonly HashSet<char> NumbersWithDot = new HashSet<char>
      {
         '0',
         '1',
         '2',
         '3',
         '4',
         '5',
         '6',
         '7',
         '8',
         '9',
         '０',
         '１',
         '２',
         '３',
         '４',
         '５',
         '６',
         '７',
         '８',
         '９',
         '.'
      };
      private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>( Path.GetInvalidFileNameChars() );

      private static readonly char[] NewlinesCharacters = new char[] { '\r', '\n' };
      private static readonly char[] WhitespacesAndNewlines = new char[] { '\r', '\n', ' ', '　' };

      public static string SanitizeForFileSystem( this string path )
      {
         var builder = new StringBuilder( path.Length );
         foreach( var c in path )
         {
            if( !InvalidFileNameChars.Contains( c ) )
            {
               builder.Append( c );
            }
         }
         return builder.ToString();
      }

      public static TemplatedString TemplatizeByNumbers( this string str )
      {
         var dict = new Dictionary<string, string>();
         bool isNumber = false;
         StringBuilder carg = null;
         char arg = 'A';

         for( int i = 0 ; i < str.Length ; i++ )
         {
            var c = str[ i ];
            if( isNumber )
            {
               if( NumbersWithDot.Contains( c ) )
               {
                  carg.Append( c );
               }
               else
               {
                  // end current number
                  var variable = carg.ToString();
                  var ok = true;
                  var c1 = variable[ 0 ];
                  if( c1 == '.' )
                  {
                     if( variable.Length == 1 )
                     {
                        ok = false;
                     }
                     else
                     {
                        var c2 = variable[ 1 ];
                        ok = Numbers.Contains( c2 );
                     }
                  }

                  if( ok && !dict.ContainsKey( variable ) )
                  {
                     dict.Add( variable, "{{" + arg + "}}" );
                     arg++;
                  }

                  carg = null;
                  isNumber = false;
               }
            }
            else
            {
               if( NumbersWithDot.Contains( c ) )
               {
                  isNumber = true;
                  carg = new StringBuilder();
                  carg.Append( c );
               }
            }
         }

         if( carg != null )
         {
            // end current number
            var variable = carg.ToString();
            var ok = true;
            var c1 = variable[ 0 ];
            if( c1 == '.' )
            {
               if( variable.Length == 1 )
               {
                  ok = false;
               }
               else
               {
                  var c2 = variable[ 1 ];
                  ok = Numbers.Contains( c2 );
               }
            }

            if( ok && !dict.ContainsKey( variable ) )
            {
               dict.Add( variable, "{{" + arg + "}}" );
               arg++;
            }
         }

         if( dict.Count > 0 )
         {
            foreach( var kvp in dict )
            {
               str = str.Replace( kvp.Key, kvp.Value );
            }

            return new TemplatedString( str, dict.ToDictionary( x => x.Value, x => x.Key ) );
         }
         else
         {
            return null;
         }
      }

      public static string SplitToLines( this string text, int maxStringLength, params char[] splitOnCharacters )
      {
         var sb = new StringBuilder();
         var index = 0;

         while( text.Length > index )
         {
            // start a new line, unless we've just started
            if( index != 0 )
               sb.Append( '\n' );

            // get the next substring, else the rest of the string if remainder is shorter than `maxStringLength`
            var splitAt = index + maxStringLength <= text.Length
                ? text.Substring( index, maxStringLength ).LastIndexOfAny( splitOnCharacters )
                : text.Length - index;

            // if can't find split location, take `maxStringLength` characters
            splitAt = ( splitAt == -1 ) ? maxStringLength : splitAt;

            // add result to collection & increment index
            sb.Append( text.Substring( index, splitAt ).Trim() );
            index += splitAt;
         }

         return sb.ToString();
      }

      public static string TrimIfConfigured( this string text )
      {
         if( text == null ) return text;

         if( Settings.TrimAllText )
         {
            return text.Trim();
         }
         return text;
      }

      public static string RemoveWhitespaceAndNewlines( this string text )
      {
         var builder = new StringBuilder( text.Length );
         if( Settings.WhitespaceRemovalStrategy == WhitespaceHandlingStrategy.AllOccurrences )
         {
            for( int i = 0 ; i < text.Length ; i++ )
            {
               var c = text[ i ];
               switch( c )
               {
                  case '\n':
                  case '\r':
                  case ' ':
                  case '　':
                     break;
                  default:
                     builder.Append( c );
                     break;
               }
            }
         }
         else // if( Settings.WhitespaceHandlingStrategy == WhitespaceHandlingStrategy.TrimPerNewline )
         {
            var lines = text.Split( NewlinesCharacters, StringSplitOptions.RemoveEmptyEntries );
            var lastLine = lines.Length - 1;
            for( int i = 0 ; i < lines.Length ; i++ )
            {
               var line = lines[ i ].Trim( WhitespacesAndNewlines );
               for( int j = 0 ; j < line.Length ; j++ )
               {
                  var c = line[ j ];
                  builder.Append( c );
               }

               // do we need to add a space when merging lines?
               if( Settings.UsesWhitespaceBetweenWords && i != lastLine ) // en, ru, ko?
               {
                  builder.Append( ' ' );
               }
            }
         }
         return builder.ToString();
      }

      public static bool StartsWithStrict( this string str, string prefix )
      {
         var len = Math.Min( str.Length, prefix.Length );
         if( len < prefix.Length ) return false;

         for( int i = 0 ; i < len ; i++ )
         {
            if( str[ i ] != prefix[ i ] ) return false;
         }

         return true;
      }

      public static string GetBetween( this string strSource, string strStart, string strEnd )
      {
         const int kNotFound = -1;

         var startIdx = strSource.IndexOf( strStart );
         if( startIdx != kNotFound )
         {
            startIdx += strStart.Length;
            var endIdx = strSource.IndexOf( strEnd, startIdx );
            if( endIdx > startIdx )
            {
               return strSource.Substring( startIdx, endIdx - startIdx );
            }
         }
         return string.Empty;
      }

      public static bool RemindsOf( this string that, string other )
      {
         return that.StartsWith( other ) || other.StartsWith( that ) || that.EndsWith( other ) || other.EndsWith( that );
      }
   }
}
