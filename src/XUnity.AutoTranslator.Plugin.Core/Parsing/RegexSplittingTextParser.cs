using System.Collections.Generic;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

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
         return !ui.IsSpammingComponent();
      }

      public ParserResult Parse( string input, int scope )
      {
         if( _cache.TryGetTranslationSplitter( input, scope, out var match, out var splitter ) )
         {
            var args = new Dictionary<string, string>();

            var groups = match.Groups;
            var len = groups.Count;
            for( int j = 1; j < len; j++ )
            {
               var group = groups[ j ];
               var groupName = "$" + j;
               var value = group.Value;
               args.Add( groupName, value );
            }

            return new ParserResult( input, splitter.Translation, true, true, Settings.CacheRegexPatternResults, true, args );
         }

         return null;
      }
   }
}
