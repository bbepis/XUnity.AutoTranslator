using System.Text.RegularExpressions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class CompositeTextTranslationCache : IReadOnlyTextTranslationCache
   {
      private IReadOnlyTextTranslationCache _first;
      private IReadOnlyTextTranslationCache _second;

      public CompositeTextTranslationCache(
         IReadOnlyTextTranslationCache first,
         IReadOnlyTextTranslationCache second )
      {
         _first = first;
         _second = second;
      }

      public bool AllowGeneratingNewTranslations => _first.AllowFallback;

      public bool AllowFallback => _first.AllowFallback;

      public bool IsTranslatable( string text, bool isToken, int scope )
      {
         return _first.IsTranslatable( text, isToken, scope )
            || ( _first.AllowFallback && _second.IsTranslatable( text, isToken, scope ) );
      }

      public bool IsPartial( string text, int scope )
      {
         return _first.IsPartial( text, scope )
            || ( _first.AllowFallback && _second.IsPartial( text, scope ) );
      }

      public bool TryGetTranslation( UntranslatedText key, bool allowRegex, bool allowToken, int scope, out string value )
      {
         return _first.TryGetTranslation( key, allowRegex, allowToken, scope, out value )
            || ( _first.AllowFallback && _second.TryGetTranslation( key, allowRegex, allowToken, scope, out value ) );
      }

      public bool TryGetTranslationSplitter( string text, int scope, out Match match, out RegexTranslationSplitter splitter )
      {
         return _first.TryGetTranslationSplitter( text, scope, out match, out splitter )
            || ( _first.AllowFallback && _second.TryGetTranslationSplitter( text, scope, out match, out splitter ) );
      }
   }
}
