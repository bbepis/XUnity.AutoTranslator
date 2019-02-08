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

      public IEnumerator Translate( string[] untranslatedTexts, string from, string to, Action<string[]> success, Action<string, Exception> failure )
      {
         var context = new TranslationContext( untranslatedTexts, from, to, success, failure );
         _ongoingTranslations++;

         bool ok = false;
         IEnumerator iterator = null;
         try
         {
            iterator = Endpoint.Translate( context );
            if( iterator != null )
            {
               TryMe: try
               {
                  ok = iterator.MoveNext();
               }
               catch( TranslationContextException )
               {
                  ok = false;
               }
               catch( Exception e )
               {
                  ok = false;
                  context.FailWithoutThrowing( "Error occurred during translation.", e );
               }

               if( ok )
               {
                  yield return iterator.Current;
                  goto TryMe;
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
