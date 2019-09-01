using System.Collections.Generic;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Parsing
{
   internal class RegexSplittingTextParser : ITextParser
   {
      public bool CanApply( object ui )
      {
         return !ui.IsSpammingComponent();
      }

      public ParserResult Parse( string input, int scope )
      {
         var patterns = Settings.Patterns;
         var length = patterns.Count;
         for( int i = 0; i < length; i++ )
         {
            var regex = patterns[ i ];
            var m = regex.CompiledRegex.Match( input );
            if( m.Success )
            {
               var args = new Dictionary<string, string>();

               var groups = m.Groups;
               var len = groups.Count;
               for( int j = 1; j < len; j++ )
               {
                  var group = groups[ j ];
                  var groupName = "$" + j;
                  var value = group.Value;
                  args.Add( groupName, value );
               }

               return new ParserResult( input, regex.Translation, true, Settings.CacheRegexPatternResults, true, args );
            }
         }

         return null;
      }
   }
}
