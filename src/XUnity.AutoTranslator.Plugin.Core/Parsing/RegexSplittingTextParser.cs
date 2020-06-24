using System.Collections.Generic;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Shims;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal class RegexSplittingTextParser : ITextParser
   {
      private readonly TextTranslationCache _cache;

      public RegexSplittingTextParser( TextTranslationCache cache )
      {
         _cache = cache;
      }

      public bool CanApply( object ui )
      {
         return !TextComponentHelper.Instance.IsSpammingComponent( ui );
      }

      public ParserResult Parse( string input, int scope )
      {
         if( _cache.TryGetTranslationSplitter( input, scope, out var match, out var splitter ) )
         {
            return new ParserResult( ParserResultOrigin.RegexTextParser, input, splitter.Translation, true, true, Settings.CacheRegexPatternResults, true, splitter.CompiledRegex, match );
         }

         return null;
      }
   }
}
