using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal class ConfiguredEndpoint
   {
      private int _ongoingTranslations;
      private Dictionary<string, byte> _failedTranslations;

      public ConfiguredEndpoint( ITranslateEndpoint endpoint, Exception error )
      {
         Endpoint = endpoint;
         Error = error;
         _ongoingTranslations = 0;
         _failedTranslations = new Dictionary<string, byte>();
      }

      public ITranslateEndpoint Endpoint { get; }

      public Exception Error { get; }

      public bool IsBusy => _ongoingTranslations >= Endpoint.MaxConcurrency;

      public bool CanTranslate( string untranslatedText )
      {
         if( _failedTranslations.TryGetValue( untranslatedText, out var count ) )
         {
            return count < Settings.MaxFailuresForSameTextPerEndpoint;
         }
         return true;
      }

      public void RegisterTranslationFailureFor( string untranslatedText )
      {
         byte count;
         if( !_failedTranslations.TryGetValue( untranslatedText, out count ) )
         {
            count = 1;
         }
         else
         {
            count++;
         }

         _failedTranslations[ untranslatedText ] = count;
      }

      public IEnumerator Translate( string[] untranslatedTexts, string from, string to, Action<string[]> success, Action<string, Exception> failure )
      {
         var startTime = Time.realtimeSinceStartup;
         var context = new TranslationContext( untranslatedTexts, from, to, success, failure );
         _ongoingTranslations++;

         try
         {
            bool ok = false;
            var iterator = Endpoint.Translate( context );
            if( iterator != null )
            {
               TryMe: try
               {
                  ok = iterator.MoveNext();

                  // check for timeout
                  var now = Time.realtimeSinceStartup;
                  if( now - startTime > Settings.Timeout )
                  {
                     ok = false;
                     context.FailWithoutThrowing( $"Timeout occurred during translation (took more than {Settings.Timeout} seconds)", null );
                  }
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
