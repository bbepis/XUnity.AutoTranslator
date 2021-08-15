using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class TemplatingHelper
   {
      public static bool ContainsUntemplatedCharacters( string text )
      {
         bool isParameter = false;
         var len = text.Length;
         for( int i = 0; i < len; i++ )
         {
            var c = text[ i ];

            if( isParameter )
            {
               if( c == '}' )
               {
                  var nidx = i + 1;
                  if( nidx < len && text[ nidx ] == '}' )
                  {
                     i++;
                     isParameter = false;
                  }
               }
            }
            else if( c == '{' )
            {
               var nidx = i + 1;
               if( nidx < len && text[ nidx ] == '{' )
               {
                  i++;
                  isParameter = true;
               }
            }
            else
            {
               if( !char.IsWhiteSpace( c ) )
               {
                  return true;
               }
            }
         }

         return false;
      }

      public static TemplatedString TemplatizeByReplacementsAndNumbers( this string str )
      {
         var templatedString = str.TemplatizeByReplacements();
         if( templatedString == null )
         {
            return str.TemplatizeByNumbers();
         }
         else
         {
            return templatedString.Template.TemplatizeByNumbers( templatedString );
         }
      }

      public static TemplatedString TemplatizeByReplacements( this string str )
      {
         if( Settings.Replacements.Count == 0 ) return null;

         var dict = new Dictionary<string, string>();
         char arg = 'A';
         bool succeeded = false;

         foreach( var kvp in Settings.Replacements )
         {
            var original = kvp.Key;
            var replacement = kvp.Value;

            if( string.IsNullOrEmpty( replacement ) )
            {
               int idx = -1;
               while( ( idx = str.IndexOf( original ) ) != -1 )
               {
                  succeeded = true;
                  str = str.Remove( idx, original.Length );
               }
            }
            else
            {
               string key = null;
               int idx = -1;
               while( ( idx = str.IndexOf( original ) ) != -1 )
               {
                  if( key == null )
                  {
                     key = "{{" + arg++ + "}}";
                     dict.Add( key, replacement );
                  }

                  str = str.Remove( idx, original.Length ).Insert( idx, key );
               }
            }
         }

         if( dict.Count > 0 || succeeded )
         {
            return new TemplatedString( str, dict );
         }
         else
         {
            return null;
         }
      }

      public static bool IsNumberOrDotOrControl( char c )
      {
         return ( '*' <= c && c <= ':' ) || ( '０' <= c && c <= '９' );
      }

      public static bool IsNumber( char c )
      {
         return ( '0' <= c && c <= '9' ) || ( '０' <= c && c <= '９' );
      }

      public static TemplatedString TemplatizeByNumbers( this string str, TemplatedString existingTemplatedString = null )
      {
         var offset = 0;
         if( existingTemplatedString != null )
         {
            offset = existingTemplatedString.Arguments.Count;
            str = existingTemplatedString.Template;
         }

         var dict = new Dictionary<string, string>();
         bool isHandlingPotentialVariable = false;
         //bool hasNumberInVariable = false;
         StringBuilder carg = null;
         char arg = (char)( 'A' + offset );
         int sidx = -1;
         int lidx = -1;
         // if it has ANY numbers of in the range, it's fine.

         // must start and end with number!

         for( int i = 0; i < str.Length; i++ )
         {
            var c = str[ i ];
            if( isHandlingPotentialVariable )
            {
               if( IsNumber( c ) )
               {
                  lidx = i;
               }
               //hasNumberInVariable = hasNumberInVariable || IsNumber( c );

               if( IsNumberOrDotOrControl( c ) )
               {
                  carg.Append( c );
               }
               else// if( hasNumberInVariable )
               {
                  var diff = i - lidx - 1;
                  carg.Remove( carg.Length - diff, diff );

                  // end current number
                  var variable = carg.ToString();
                  var argName = "{{" + arg + "}}";
                  dict.Add( argName, variable );
                  arg++;

                  carg = null;
                  isHandlingPotentialVariable = false;

                  str = str.Remove( sidx, lidx - sidx + 1 ).Insert( sidx, argName );
                  i += argName.Length - variable.Length;
               }
               //else
               //{
               //   carg = null;
               //   isHandlingPotentialVariable = false;
               //}
            }
            else if( IsNumber( c ) )
            {
               isHandlingPotentialVariable = true;
               //hasNumberInVariable = true;
               carg = new StringBuilder();
               carg.Append( c );
               sidx = i;
               lidx = i;
            }
         }

         if( carg != null )
         {
            var diff = str.Length - lidx - 1;
            carg.Remove( carg.Length - diff, diff );

            // end current number
            var variable = carg.ToString();
            var argName = "{{" + arg + "}}";
            dict.Add( argName, variable );

            str = str.Remove( sidx, str.Length - sidx - diff ).Insert( sidx, argName );
         }

         if( dict.Count > 0 )
         {
            var resultDictionary = existingTemplatedString?.Arguments ?? dict;

            if( dict != resultDictionary )
            {
               foreach( var kvp in dict )
               {
                  resultDictionary.Add( kvp.Key, kvp.Value );
               }
            }

            return new TemplatedString( str, resultDictionary );
         }
         else
         {
            return existingTemplatedString;
         }
      }
   }
}
