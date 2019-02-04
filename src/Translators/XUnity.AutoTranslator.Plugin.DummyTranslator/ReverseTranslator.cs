using System;
using System.Collections;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.DummyTranslator
{
   public class ReverseTranslator : ITranslateEndpoint
   {
      public string Id => "Reverser";

      public string FriendlyName => "Reverser";

      public int MaxConcurrency => 50;

      public void Initialize( InitializationContext context )
      {

      }

      public IEnumerator Translate( TranslationContext context )
      {
         var reversedText = new string( context.UntranslatedText.Reverse().ToArray() );
         context.Complete( reversedText );

         return null;
      }
   }
}
