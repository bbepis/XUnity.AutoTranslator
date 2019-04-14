using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class ParserTranslationContext
   {
      public ParserTranslationContext( object component, TranslationEndpointManager endpoint, TranslationResult translationResult, ParserResult result )
      {
         Jobs = new HashSet<TranslationJob>();
         Component = component;
         Result = result;
         Endpoint = endpoint;
         TranslationResult = translationResult;
      }

      public ParserResult Result { get; private set; }

      public HashSet<TranslationJob> Jobs { get; private set; }

      public TranslationResult TranslationResult { get; private set; }

      public object Component { get; private set; }

      public TranslationEndpointManager Endpoint { get; private set; }
   }
}
