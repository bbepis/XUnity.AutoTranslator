using System;
using System.Collections;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   public abstract class HttpEndpoint : ITranslateEndpoint
   {
      public abstract string Id { get; }

      public abstract string FriendlyName { get; }

      public int MaxConcurrency => 1;

      public abstract void Initialize( InitializationContext context );

      public abstract void ExtractTranslatedText( HttpTranslationContext context );

      public abstract XUnityWebRequest CreateTranslationRequest( HttpTranslationContext context );

      public virtual void InspectTranslationResponse( HttpTranslationContext context, XUnityWebResponse response )
      {
      }

      public virtual IEnumerator OnBeforeTranslate( HttpTranslationContext context ) => null;

      public IEnumerator Translate( TranslationContext context )
      {
         var httpContext = new HttpTranslationContext( context );

         // allow implementer of HttpEndpoint to do anything before starting translation
         var setup = OnBeforeTranslate( httpContext );
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
            var request = CreateTranslationRequest( httpContext );
            response = client.Send( request );
         }
         catch( Exception e )
         {
            httpContext.Fail( "Error occurred while setting up translation request.", e );
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

         InspectTranslationResponse( httpContext, response );

         // failure
         if( response.Error != null )
         {
            httpContext.Fail( "Error occurred while retrieving translation.", response.Error );
            yield break;
         }

         // failure
         if( response.Result == null )
         {
            httpContext.Fail( "Error occurred while retrieving translation. Nothing was returned.", null );
            yield break;
         }

         httpContext.ResultData = response.Result;

         try
         {
            // attempt to extract translation from data
            ExtractTranslatedText( httpContext );
         }
         catch( Exception e )
         {
            httpContext.Fail( "Error occurred while retrieving translation.", e );
         }
      }
   }
}
