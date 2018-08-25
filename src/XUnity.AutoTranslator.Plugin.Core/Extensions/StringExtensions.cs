using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class StringExtensions
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

      private static readonly char[] NewlinesCharacters = new char[] { '\r', '\n' };
      private static readonly char[] WhitespacesAndNewlines = new char[] { '\r', '\n', ' ', '　' };

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

               // if source language is ENGLISH; Add space, if not last line, so words are not merged
               if( Settings.FromLanguage == Settings.EnglishLanguage && i != lastLine )
               {
                  builder.Append( ' ' );
               }
            }
         }
         return builder.ToString();
      }

      public static bool ContainsNumbers( this string text )
      {
         foreach( var c in text )
         {
            if( Numbers.Contains( c ) )
            {
               return true;
            }
         }
         return false;
      }

      public static string UnescapeJson( this string str )
      {
         if( str == null ) return null;

         var builder = new StringBuilder( str );

         bool escapeNext = false;
         for( int i = 0 ; i < builder.Length ; i++ )
         {
            var c = builder[ i ];
            if( escapeNext )
            {
               bool found = true;
               char escapeWith = default( char );
               switch( c )
               {
                  case 'b':
                     escapeWith = '\b';
                     break;
                  case 'f':
                     escapeWith = '\f';
                     break;
                  case 'n':
                     escapeWith = '\n';
                     break;
                  case 'r':
                     escapeWith = '\r';
                     break;
                  case 't':
                     escapeWith = '\t';
                     break;
                  case '"':
                     escapeWith = '\"';
                     break;
                  case '\\':
                     escapeWith = '\\';
                     break;
                  case 'u':
                     escapeWith = 'u';
                     break;
                  default:
                     found = false;
                     break;
               }

               // remove previous char and go one back
               if( found )
               {
                  if( escapeWith == 'u' )
                  {
                     // unicode crap, lets handle the next 4 characters manually
                     int code = int.Parse( new string( new char[] { builder[ i + 1 ], builder[ i + 2 ], builder[ i + 3 ], builder[ i + 4 ] } ), NumberStyles.HexNumber );
                     var replacingChar = (char)code;
                     builder.Remove( --i, 6 );
                     builder.Insert( i, replacingChar );
                  }
                  else
                  {
                     // found proper escaping
                     builder.Remove( --i, 2 );
                     builder.Insert( i, escapeWith );
                  }
               }
               else
               {
                  // dont do anything
               }

               escapeNext = false;
            }
            else if( c == '\\' )
            {
               escapeNext = true;
            }
         }

         return builder.ToString();
      }

      public static string EscapeJson( this string str )
      {
         if( str == null || str.Length == 0 )
         {
            return "";
         }

         char c;
         int len = str.Length;
         StringBuilder sb = new StringBuilder( len + 4 );
         for( int i = 0 ; i < len ; i += 1 )
         {
            c = str[ i ];
            switch( c )
            {
               case '\\':
               case '"':
                  sb.Append( '\\' );
                  sb.Append( c );
                  break;
               case '/':
                  sb.Append( '\\' );
                  sb.Append( c );
                  break;
               case '\b':
                  sb.Append( "\\b" );
                  break;
               case '\t':
                  sb.Append( "\\t" );
                  break;
               case '\n':
                  sb.Append( "\\n" );
                  break;
               case '\f':
                  sb.Append( "\\f" );
                  break;
               case '\r':
                  sb.Append( "\\r" );
                  break;
               default:
                  sb.Append( c );
                  break;
            }
         }
         return sb.ToString();
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
         return String.Empty;
      }
   }
}
