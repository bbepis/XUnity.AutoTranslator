using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   public class ParserResult
   {
      public ParserResult( string originalText, string template, bool hasRichText, Dictionary<string, string> args )
      {
         OriginalText = originalText;
         Template = template;
         HasRichSyntax = hasRichText;
         Arguments = args;
      }

      public string OriginalText { get; private set; }

      public string Template { get; private set; }

      public Dictionary<string, string> Arguments { get; private set; }

      public bool HasRichSyntax { get; private set; }

      public string Untemplate( Dictionary<string, string> arguments )
      {
         string result = Template;
         foreach( var kvp in arguments )
         {
            result = result.Replace( kvp.Key, kvp.Value );
         }
         return result;
      }
   }
}
