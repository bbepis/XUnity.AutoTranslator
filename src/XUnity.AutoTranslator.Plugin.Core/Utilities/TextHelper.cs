using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class TextHelper
   {
      public static string Encode( string text )
      {
         return EscapeNewlines( text );
      }

      public static string[] ReadTranslationLineAndDecode( string str )
      {
         if( string.IsNullOrEmpty( str ) ) return null;

         var lines = new string[ 2 ];
         var lidx = 0;
         bool escapeNext = false;
         var len = str.Length;
         var builder = new StringBuilder( (int)( len / 1.3 ) );
         for( int i = 0; i < len; i++ )
         {
            var c = str[ i ];
            if( escapeNext )
            {
               switch( c )
               {
                  case '=':
                  case '\\':
                     builder.Append( c );
                     break;
                  case 'n':
                     builder.Append( '\n' );
                     break;
                  case 'r':
                     builder.Append( '\r' );
                     break;
                  case 'u':
                     var i4 = i + 4;
                     if( i4 < len )
                     {
                        int code = int.Parse( new string( new char[] { str[ i + 1 ], str[ i + 2 ], str[ i + 3 ], str[ i + 4 ] } ), NumberStyles.HexNumber );
                        builder.Append( (char)code );
                        i += 4;
                     }
                     else
                     {
                        throw new Exception( "Found invalid unicode in line: " + str );
                     }
                     break;
                  default:
                     builder.Append( '\\' );
                     builder.Append( c );
                     break;
               }

               escapeNext = false;
            }
            else if( c == '\\' )
            {
               escapeNext = true;
            }
            else if( c == '=' )
            {
               if( lidx > 1 )
               {
                  return null;
               }

               lines[ lidx++ ] = builder.ToString();
               builder.Length = 0;
            }
            else if( c == '%' )
            {
               // backwards compatibility for old newline encoding
               var i2 = i + 2;
               if( i2 < len && str[ i + 1 ] == '3' && str[ i + 2 ] == 'D' )
               {
                  builder.Append( '=' );
                  i += 2;
               }
               else
               {
                  builder.Append( c );
               }
            }
            else if( c == '/' )
            {
               // Maybe this is an inline comment
               var n = i + 1;
               if( n < len && str[ n ] == '/' )
               {
                  lines[ lidx++ ] = builder.ToString();

                  if( lidx == 2 )
                  {
                     return lines;
                  }
                  else
                  {
                     return null;
                  }
               }
               else
               {
                  builder.Append( c );
               }
            }
            else
            {
               builder.Append( c );
            }
         }

         if( lidx != 1 )
         {
            return null;
         }

         lines[ lidx++ ] = builder.ToString();

         return lines;
      }

      internal static string EscapeNewlines( string str )
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
               case '/':
                  var i1 = i + 1;
                  if( i1 < len && str[ i1 ] == '/' )
                  {
                     sb.Append( '\\' );
                     sb.Append( c );
                     sb.Append( '\\' );
                     sb.Append( c );
                     i++;
                  }
                  else
                  {
                     sb.Append( c );
                  }
                  break;
               case '\\':
                  sb.Append( '\\' );
                  sb.Append( c );
                  break;
               case '=':
                  sb.Append( '\\' );
                  sb.Append( c );
                  break;
               case '\n':
                  sb.Append( "\\n" );
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
