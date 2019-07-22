using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class TextHelper
   {
      internal static string Decode( string text )
      {
         var commentIndex = text.IndexOf( "//" );
         if( commentIndex > -1 )
         {
            text = text.Substring( 0, commentIndex );
         }

         return UnescapeNewlines( text )
            .Replace( "%3D", "=" )
            .Replace( "%2F%2F", "//" );
      }

      internal static string Encode( string text )
      {
         return EscapeNewlines( text )
            .Replace( "=", "%3D" )
            .Replace( "//", "%2F%2F" );
      }

      public static string UnescapeNewlines( string str )
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
                  case 'n':
                     escapeWith = '\n';
                     break;
                  case 'r':
                     escapeWith = '\r';
                     break;
                  case '\\':
                     escapeWith = '\\';
                     break;
                  default:
                     found = false;
                     break;
               }

               // remove previous char and go one back
               if( found )
               {
                  // found proper escaping
                  builder.Remove( --i, 2 );
                  builder.Insert( i, escapeWith );
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

      public static string EscapeNewlines( string str )
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


//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;

//namespace XUnity.AutoTranslator.Plugin.Core.Utilities
//{
//   internal static class TextHelper
//   {
//      internal static string Decode( string text )
//      {
//         // WONT WORK
//         var commentIndex = text.IndexOf( "//" );
//         if( commentIndex > -1 )
//         {
//            text = text.Substring( 0, commentIndex );
//         }

//         return Unescape( text )
//            .Replace( "%3D", "=" )
//            .Replace( "%2F%2F", "//" );
//      }

//      internal static string Encode( string text )
//      {
//         // FIXME: Wont work because we break on "=" outside

//         return Escape( text );
//         //.Replace( "=", "%3D" )
//         //.Replace( "//", "%2F%2F" );
//      }

//      public static string Unescape( string text )
//      {
//         if( text == null ) return null;

//         var builder = new StringBuilder( text );
//         string translatedText = null;
//         string untranslatedText = null;

//         bool escapeNext = false;
//         for( int i = 0; i < builder.Length; i++ )
//         {
//            var c = builder[ i ];
//            if( escapeNext )
//            {
//               bool found = true;
//               char escapeWith = default( char );
//               string escapeWithString = null;
//               switch( c )
//               {
//                  case '\\':
//                     escapeWith = '\\';
//                     break;
//                  case 'n':
//                     escapeWith = '\n';
//                     break;
//                  case 'r':
//                     escapeWith = '\r';
//                     break;
//                  case '=':
//                     escapeWith = '=';
//                     break;
//                  case '/':
//                     if( builder.Length > i + 1 )
//                     {
//                        var nc = builder[ i + 1 ];
//                        if( nc == '/' )
//                        {
//                           escapeWithString = "//";
//                           i++; // skip next char...
//                        }
//                     }
//                     break;
//                  default:
//                     found = false;
//                     break;
//               }

//               // remove previous char and go one back
//               if( found )
//               {
//                  if( escapeWithString != null )
//                  {
//                     i -= escapeWithString.Length;

//                     builder.Remove( i, 1 + escapeWithString.Length );
//                     builder.Insert( i, escapeWith );
//                  }
//                  else
//                  {
//                     builder.Remove( --i, 2 );
//                     builder.Insert( i, escapeWith );
//                  }
//               }

//               escapeNext = false;
//            }
//            else if( c == '\\' )
//            {
//               escapeNext = true;
//            }
//            else if( c == '/' )
//            {
//               if( builder.Length > i + 1 )
//               {
//                  var nc = builder[ i + 1 ];
//                  if( nc == '/' )
//                  {
//                     builder.Remove( i, builder.Length - i ); // remove the rest of the string

//                     // TODO: Could also remove any ' ' leading up to it!

//                     break;
//                  }
//               }
//            }
//            else if( c == '=' && untranslatedText == null )
//            {
//               untranslatedText = builder.ToString( 0, i - 1 ); // exclude '='
//               builder.Remove( 0, i ); // include "="
//            }
//         }

//         translatedText = builder.ToString();

//         if( untranslatedText != null && translatedText != null )
//         {
//            // fix url encoded characters in both (legacy support)
//         }
//         else
//         {
//            return null;
//         }
//      }

//      public static string Escape( string str )
//      {
//         if( str == null || str.Length == 0 )
//         {
//            return "";
//         }

//         char c;
//         int len = str.Length;
//         StringBuilder sb = new StringBuilder( len + 4 );
//         for( int i = 0; i < len; i++ )
//         {
//            c = str[ i ];
//            switch( c )
//            {
//               case '\\':
//                  sb.Append( '\\' );
//                  sb.Append( c );
//                  break;
//               case '\n':
//                  sb.Append( "\\n" );
//                  break;
//               case '\r':
//                  sb.Append( "\\r" );
//                  break;
//               case '=':
//                  sb.Append( "\\=" );
//                  break;
//               case '/':
//                  if( len > i + 1 )
//                  {
//                     var nc = str[ i + 1 ];
//                     if( nc == '/' )
//                     {
//                        sb.Append( "\\//" );
//                        i++; // skip next char...
//                     }
//                  }
//                  else
//                  {
//                     sb.Append( c );
//                  }
//                  break;
//               default:
//                  sb.Append( c );
//                  break;
//            }
//         }
//         return sb.ToString();
//      }
//   }
//}
