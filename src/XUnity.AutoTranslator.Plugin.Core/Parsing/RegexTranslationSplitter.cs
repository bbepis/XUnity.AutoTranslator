using System;
using System.Text.RegularExpressions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal class RegexTranslationSplitter
   {
      public RegexTranslationSplitter( string key, string value )
      {
         Key = key;
         Value = value;

         // remove sr:
         if( key.StartsWith( "sr:" ) )
         {
            key = key.Substring( 3, key.Length - 3 );
         }

         var startIdx = key.IndexOf( '"' );
         if( startIdx == -1 )
         {
            // take entire string
         }
         else
         {
            startIdx++;
            var endIdx = key.LastIndexOf( '"' );
            if( endIdx != startIdx - 1 )
            {
               key = key.Substring( startIdx, endIdx - startIdx );
            }
            else
            {
               throw new Exception( $"Regex with key: '{Key}' starts with a \" but does not end with a \"." );
            }
         }

         // remove sr:
         if( value.StartsWith( "sr:" ) )
         {
            value = value.Substring( 3, value.Length - 3 );
         }

         startIdx = value.IndexOf( '"' );
         if( startIdx == -1 )
         {
            // take entire string
         }
         else
         {
            startIdx++;
            var endIdx = value.LastIndexOf( '"' );
            if( endIdx != startIdx - 1 )
            {
               value = value.Substring( startIdx, endIdx - startIdx );
            }
            else
            {
               throw new Exception( $"Regex with value: '{Value}' starts with a \" but does not end with a \"." );
            }
         }

         if( !key.StartsWith( "^" ) ) key = "^" + key;
         if( !key.EndsWith( "$" ) ) key = key + "$";

         CompiledRegex = new Regex( key );
         Original = key;
         Translation = value;
      }

      public Regex CompiledRegex { get; set; }

      public string Original { get; set; }

      public string Translation { get; set; }

      public string Key { get; set; }

      public string Value { get; set; }
   }
}
