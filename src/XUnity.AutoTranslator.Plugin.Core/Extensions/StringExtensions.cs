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

      public static string ChangeToSingleLineForDialogue( this string that )
      {
         if( that.Length > Settings.MinDialogueChars ) // long strings often indicate dialog
         {
            // Always change dialogue into one line. Otherwise translation services gets confused.
            return that.RemoveWhitespace();
         }
         else
         {
            return that;
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

      public static string RemoveWhitespace( this string text )
      {
         // Japanese whitespace, wtf
         return text.Replace( "\n", "" ).Replace( "\r", "" ).Replace( " ", "" ).Replace( "　", "" );
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
   }
}
