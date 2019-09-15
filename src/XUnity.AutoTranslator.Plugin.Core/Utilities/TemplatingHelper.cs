using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class TemplatingHelper
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

      public static TemplatedString TemplatizeByReplacements( this string str )
      {
         if( Settings.Replacements.Count == 0 ) return null;

         var dict = new Dictionary<string, string>();
         char arg = 'A';

         foreach( var kvp in Settings.Replacements )
         {
            var original = kvp.Key;
            var replacement = kvp.Value;

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

         if( dict.Count > 0 )
         {
            return new TemplatedString( str, dict );
         }
         else
         {
            return null;
         }
      }

      public static TemplatedString TemplatizeByNumbers( this string str )
      {
         var dict = new Dictionary<string, string>();
         bool isNumber = false;
         StringBuilder carg = null;
         char arg = 'A';

         for( int i = 0; i < str.Length; i++ )
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
   }
}
