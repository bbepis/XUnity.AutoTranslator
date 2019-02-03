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

      public IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action<string, Exception> failure )
      {
         success( new string( untranslatedText.Reverse().ToArray() ) );

         return null;
      }
   }
}
