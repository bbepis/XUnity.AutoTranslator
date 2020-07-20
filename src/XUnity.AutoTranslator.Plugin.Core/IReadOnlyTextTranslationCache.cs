using System.Text.RegularExpressions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal interface IReadOnlyTextTranslationCache
   {
      bool AllowGeneratingNewTranslations { get; }

      bool AllowFallback { get; }

      bool IsTranslatable( string text, bool isToken, int scope );

      bool IsPartial( string text, int scope );

      bool TryGetTranslation( UntranslatedText key, bool allowRegex, bool allowToken, int scope, out string value );

      bool TryGetTranslationSplitter( string text, int scope, out Match match, out RegexTranslationSplitter splitter );
   }
}
