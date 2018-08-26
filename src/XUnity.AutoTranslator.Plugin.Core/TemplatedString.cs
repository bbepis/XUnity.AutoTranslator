using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class TemplatedString
   {
      public TemplatedString( string template, Dictionary<string, string> arguments )
      {
         Template = template;
         Arguments = arguments;
      }

      public string Template { get; private set; }

      public Dictionary<string, string> Arguments { get; private set; }

      public string Untemplate( string text )
      {
         foreach( var kvp in Arguments )
         {
            text = text.Replace( kvp.Key, kvp.Value );
         }
         return text;
      }

      public string RepairTemplate( string text )
      {
         // TODO: Implement template repairation. The web services might have mangled our parameterization
         return text;
      }
   }
}
