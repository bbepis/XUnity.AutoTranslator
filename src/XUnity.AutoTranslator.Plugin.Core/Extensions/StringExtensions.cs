using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class StringExtensions
   {
      public static string ChangeToSingleLineForDialogue( this string that )
      {
         if( that.Length > 18 ) // long strings often indicate dialog
         {
            // Always change dialogue into one line. Otherwise translation services gets confused.
            return that.RemoveNewlines();
         }
         else
         {
            return that;
         }
      }

      public static string RemoveNewlines( this string text )
      {
         return text.Replace( "\n", "" ).Replace( "\r", "" );
      }

      public static string RemoveWhitespace( this string text )
      {
         // Japanese whitespace, wtf
         return text.RemoveNewlines().Replace( " ", "" ).Replace( "　", "" );
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
