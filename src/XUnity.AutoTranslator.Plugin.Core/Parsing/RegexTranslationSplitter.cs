using System.Text.RegularExpressions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal class RegexTranslationSplitter
   {
      public RegexTranslationSplitter( string regex, string translation )
      {
         CompiledRegex = new Regex( regex );
         Translation = translation;
         Original = regex;
      }

      public Regex CompiledRegex { get; set; }

      public string Original { get; set; }

      public string Translation { get; set; }
   }
}
