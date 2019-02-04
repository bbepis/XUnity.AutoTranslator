using System;
using System.Collections;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal class ConfiguredEndpoint
   {
      private int _ongoingTranslations;

      public ConfiguredEndpoint( ITranslateEndpoint endpoint, Exception error )
      {
         Endpoint = endpoint;
         Error = error;
         _ongoingTranslations = 0;
      }

      public ITranslateEndpoint Endpoint { get; }

      public Exception Error { get; }

      public bool IsBusy => _ongoingTranslations >= Endpoint.MaxConcurrency;

      public IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action<string, Exception> failure )
      {
         var context = new TranslationContext( untranslatedText, from, to, success, failure );
         _ongoingTranslations++;
         try
         {
            var iterator = Endpoint.Translate( context );
            if( iterator != null )
            {
               while( iterator.MoveNext() )
               {
                  yield return iterator.Current;
               }
            }
         }
         finally
         {
            _ongoingTranslations--;

            context.FailIfNotCompleted();
         }
      }
   }
}
