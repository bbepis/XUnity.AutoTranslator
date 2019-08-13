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

         var startIdx = key.IndexOf( '"' ) + 1;
         if( startIdx == -1 )
         {
            // take entire string
         }
         else
         {
            var endIdx = key.LastIndexOf( '"', key.Length - 1 );
            if( endIdx != startIdx )
            {
               key = key.Substring( startIdx, endIdx - startIdx );
            }
         }

         // remove r:
         if( value.StartsWith( "r:" ) )
         {
            value = value.Substring( 2, value.Length - 2 );
         }

         startIdx = value.IndexOf( '"' ) + 1;
         if( startIdx == -1 )
         {
            // take entire string
         }
         else
         {
            var endIdx = value.LastIndexOf( '"', value.Length - 1 );
            if( endIdx != startIdx )
            {
               value = value.Substring( startIdx, endIdx - startIdx );
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
