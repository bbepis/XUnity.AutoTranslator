using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ExtProtocol.Utilities
{
   /// <summary>
   /// Helper class to work with json, because the json
   /// serializers for Unity sucks... badly.
   /// </summary>
   public static class JsonHelper
   {
      /// <summary>
      /// Unescapes a json string.
      /// </summary>
      /// <param name="str"></param>
      /// <returns></returns>
      public static string Unescape( string str )
      {
         if( str == null ) return null;

         var builder = new StringBuilder( str );

         bool escapeNext = false;
         for( int i = 0; i < builder.Length; i++ )
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

               escapeNext = false;
            }
            else if( c == '\\' )
            {
               escapeNext = true;
            }
         }

         return builder.ToString();
      }

      /// <summary>
      /// Escapes a json string.
      /// </summary>
      /// <param name="str"></param>
      /// <returns></returns>
      public static string Escape( string str )
      {
         if( str == null || str.Length == 0 )
         {
            return "";
         }

         char c;
         int len = str.Length;
         StringBuilder sb = new StringBuilder( len + 4 );
         for( int i = 0; i < len; i += 1 )
         {
            c = str[ i ];
            switch( c )
            {
               case '\\':
               case '"':
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
               case '\u0085': // Next Line
                  sb.Append( "\\u0085" );
                  break;
               case '\u2028': // Line Separator
                  sb.Append( "\\u2028" );
                  break;
               case '\u2029': // Paragraph Separator
                  sb.Append( "\\u2029" );
                  break;
               default:
                  sb.Append( c );
                  break;
            }
         }
         return sb.ToString();
      }
   }
}
