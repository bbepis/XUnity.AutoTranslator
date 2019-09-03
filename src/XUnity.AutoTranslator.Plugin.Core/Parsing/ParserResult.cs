using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal class ParserResult
   {
      public ParserResult( string originalText, string template, bool allowPartialTranslation, bool cacheCombinedResult, bool persistCombinedResult, bool persistTokenResult, Dictionary<string, string> args )
      {
         OriginalText = originalText;
         Template = template;
         AllowPartialTranslation = allowPartialTranslation;
         CacheCombinedResult = cacheCombinedResult;
         PersistCombinedResult = persistCombinedResult;
         PersistTokenResult = persistTokenResult;
         Arguments = args;
      }

      public string OriginalText { get; }

      public string Template { get; }
      public Dictionary<string, string> Arguments { get; }

      public bool AllowPartialTranslation { get; }

      public bool CacheCombinedResult { get; }

      public bool PersistCombinedResult { get; }

      public bool PersistTokenResult { get; }

      public string Untemplate( Dictionary<string, string> arguments )
      {
         // This is really not a nice fix...
         if( arguments.Count > 9 )
         {
            var result = new StringBuilder( Template );
            foreach( var kvp in arguments.OrderByDescending( x => x.Key.Length ) )
            {
               result = result.Replace( kvp.Key, kvp.Value );
            }
            return result.ToString();
         }
         else
         {
            var result = new StringBuilder( Template );
            foreach( var kvp in arguments )
            {
               result = result.Replace( kvp.Key, kvp.Value );
            }
            return result.ToString();
         }
      }
   }
}
