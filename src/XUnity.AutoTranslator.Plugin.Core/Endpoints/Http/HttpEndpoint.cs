using System;
using System.Collections;
using System.Net;
using System.Threading;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Shim;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   public abstract class HttpEndpoint : ITranslateEndpoint
   {
      public abstract string Id { get; }

      public abstract string FriendlyName { get; }

      public XUnityWebClient Client { get; }

      public int MaxConcurrency => 1;

      public abstract void Initialize( InitializationContext context );

      public abstract bool TryExtractTranslated( string result, out string translated );

      public abstract XUnityWebRequest CreateTranslationRequest( string untranslatedText, string from, string to );

      public virtual void InspectTranslationResponse( XUnityWebResponse response )
      {
      }

      public virtual IEnumerator OnBeforeTranslate() => null;

      public IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action<string, Exception> failure )
      {
         // allow implementer of HttpEndpoint to do anything before starting translation
         var setup = OnBeforeTranslate();
         if( setup != null )
         {
            while( setup.MoveNext() )
            {
               yield return setup.Current;
            }
         }

         XUnityWebResponse response = null;
         try
         {
            // prepare request
            var client = new XUnityWebClient();
            var request = CreateTranslationRequest( untranslatedText, from, to );
            response = client.Send( request );
         }
         catch( Exception e )
         {
            failure( "Error occurred while setting up translation request.", e );
            yield break;
         }

         // wait for completion
         if( Features.SupportsCustomYieldInstruction )
         {
            yield return response;
         }
         else
         {
            while( !response.IsCompleted )
            {
               yield return new WaitForSeconds( 0.2f );
            }
         }

         InspectTranslationResponse( response );

         // failure
         if( response.Error != null )
         {
            failure( "Error occurred while retrieving translation.", response.Error );
            yield break;
         }

         // failure
         if( response.Result == null )
         {
            failure( "Error occurred while retrieving translation. Nothing was returned.", null );
            yield break;
         }


         try
         {
            // attempt to extract translation from data
            if( TryExtractTranslated( response.Result, out var translatedText ) )
            {
               translatedText = translatedText ?? string.Empty;
               success( translatedText );
            }
            else
            {
               failure( "Error occurred while extracting translation.", null );
            }
         }
         catch( Exception e )
         {
            failure( "Error occurred while retrieving translation.", e );
         }
      }
   }
}
