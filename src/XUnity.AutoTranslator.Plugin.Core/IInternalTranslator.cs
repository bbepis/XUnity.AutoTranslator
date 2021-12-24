using System;
using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core
{
   interface IInternalTranslator : ITranslator
   {
      void TranslateAsync( TranslationEndpointManager endpoint, string untranslatedText, Action<TranslationResult> onCompleted );
   }
}
