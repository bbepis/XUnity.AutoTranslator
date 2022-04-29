using System;
using System.Text.RegularExpressions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   class RegexTranslation
   {
      public RegexTranslation( string key, string value )
      {
         Key = key;
         Value = value;

         // remove r:
         if( key.StartsWith( "r:" ) )
         {
            key = key.Substring( 2, key.Length - 2 );
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
               throw new Exception( $"Splitter regex with key: '{Key}' starts with a \" but does not end with a \"." );
            }
         }

         // remove r:
         if( value.StartsWith( "r:" ) )
         {
            value = value.Substring( 2, value.Length - 2 );
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
            if( endIdx != startIdx - 1)
            {
               value = value.Substring( startIdx, endIdx - startIdx );
            }
            else
            {
               throw new Exception( $"Splitter regex with value: '{Value}' starts with a \" but does not end with a \"." );
            }
         }

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
