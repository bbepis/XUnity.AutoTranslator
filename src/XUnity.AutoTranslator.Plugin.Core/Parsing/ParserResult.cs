using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   public class ParserResult
   {
      public ParserResult( string template, Dictionary<string, string> args )
      {
         Template = template;
         Arguments = args;
      }

      public string Template { get; private set; }

      public Dictionary<string, string> Arguments { get; private set; }

      public bool HasRichSyntax => Template.Length > 5; // {{A}} <-- 5 chars

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
