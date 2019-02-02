using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TranslationContext
   {
      public TranslationContext( object component, ParserResult result )
      {
         Jobs = new HashSet<TranslationJob>();
         Component = component;
         Result = result;
      }

      public ParserResult Result { get; private set; }

      public HashSet<TranslationJob> Jobs { get; private set; }

      public object Component { get; private set; }
   }
}
