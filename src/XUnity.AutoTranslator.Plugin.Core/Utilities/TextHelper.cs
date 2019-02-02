using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   public static class TextHelper
   {
      /// <summary>
      /// Decodes a text from a single-line serializable format.
      /// 
      /// Shamelessly stolen from original translation plugin.
      /// </summary>
      public static string Decode( string text )
      {
         var commentIndex = text.IndexOf( "//" );
         if( commentIndex > -1 )
         {
            text = text.Substring( 0, commentIndex );
         }

         return text.Replace( "\\r", "\r" )
            .Replace( "\\n", "\n" )
            .Replace( "%3D", "=" )
            .Replace( "%2F%2F", "//" );
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
            .Replace( "=", "%3D" )
            .Replace( "//", "%2F%2F" );
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
   }
}
