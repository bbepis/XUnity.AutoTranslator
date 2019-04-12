using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class ParserTranslationContext
   {
      public ParserTranslationContext( object component, ParserResult result )
      {
         Jobs = new HashSet<TranslationJob>();
         Component = component;
         Result = result;
      }

      public ParserResult Result { get; private set; }

      public HashSet<TranslationJob> Jobs { get; private set; }

      public object Component { get; private set; }

      public TranslationEndpointManager Endpoint => Jobs.FirstOrDefault()?.Endpoint;
   }
}
