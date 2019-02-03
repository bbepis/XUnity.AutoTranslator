using System;
using System.Collections;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.DummyTranslator
{
   public class DummyTranslator : ITranslateEndpoint
   {
      public string Id => "Dummy";

      public string FriendlyName => "Dummy";

      public int MaxConcurrency => 50;

      public void Initialize( InitializationContext context )
      {

      }

      public IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action<string, Exception> failure )
      {
         success( "Incorrect translation" );

         return null;
      }
   }
}
