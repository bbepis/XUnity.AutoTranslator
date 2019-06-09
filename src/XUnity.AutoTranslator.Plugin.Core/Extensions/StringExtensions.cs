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

      private static readonly string[] NewlinesCharacters = new string[] { "\r\n", "\n" };
      private static readonly char[] WhitespacesAndNewlines = new char[] { '\r', '\n', ' ', '　' };
      private static readonly char[] Spaces = new char[] { ' ', '　' };

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

      public static bool StartsWithStrict( this string str, string prefix )
      {
         var len = Math.Min( str.Length, prefix.Length );
         if( len < prefix.Length ) return false;

         for( int i = 0; i < len; i++ )
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
