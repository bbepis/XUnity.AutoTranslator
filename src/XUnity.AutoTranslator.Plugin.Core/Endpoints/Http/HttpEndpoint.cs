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
      public HttpEndpoint()
      {
         Client = new UnityWebClient( this );
      }

      public abstract string Id { get; }

      public abstract string FriendlyName { get; }

      public UnityWebClient Client { get; }

      public abstract void Initialize( IConfiguration configuration, ServiceEndpointConfiguration servicePoints );

      public abstract string GetServiceUrl( string untranslatedText, string from, string to );

      public abstract bool TryExtractTranslated( string result, out string translated );

      public int MaxConcurrency => 1;

      public virtual string GetRequestObject( string untranslatedText, string from, string to ) => null;

      public virtual CookieContainer GetCookiesForNewRequest() => null;

      public virtual IEnumerator OnBeforeTranslate() => null;

      public virtual void StoreCookiesFromResponse( HttpWebResponse response )
      {

      }

      public virtual void ApplyHeaders( WebHeaderCollection headers )
      {

      }

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

         UnityWebResponse response = null;
         try
         {
            // prepare request
            var url = GetServiceUrl( untranslatedText, from, to );
            var request = GetRequestObject( untranslatedText, from, to );
            ApplyHeaders( Client.Headers );

            // execute request
            if( request != null )
            {
               response = Client.UploadStringByUnityInstruction( new Uri( url ), request );
            }
            else
            {
               response = Client.DownloadStringByUnityInstruction( new Uri( url ) );
            }
         }
         catch( Exception e )
         {
            failure( "Error occurred while setting up translation request.", e );
            yield break;
         }

         if( response == null )
         {
            failure( "Unexpected error occurred while retrieving translation.", null );
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

         // failure
         if( !response.Succeeded || response.Result == null )
         {
            failure( "Error occurred while retrieving translation." + Environment.NewLine + response.Error, null );
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
