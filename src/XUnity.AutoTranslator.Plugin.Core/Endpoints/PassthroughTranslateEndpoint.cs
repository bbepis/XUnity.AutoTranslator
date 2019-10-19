using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal sealed class PassthroughTranslateEndpoint : ITranslateEndpoint
   {
      public string Id => "Passthrough";

      public string FriendlyName => "Passthrough";

      public int MaxConcurrency => 50;

      public int MaxTranslationsPerRequest => 50;

      public void Initialize( IInitializationContext context )
      {
      }

      public IEnumerator Translate( ITranslationContext context )
      {
         context.Complete( context.UntranslatedTexts );

         return null;
      }
   }
}
