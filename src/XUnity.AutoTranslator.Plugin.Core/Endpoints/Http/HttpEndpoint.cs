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

      public abstract void Initialize( IInitializationContext context );

      public abstract void OnExtractTranslation( IHttpTranslationExtractionContext context );

      public abstract void OnCreateRequest( IHttpRequestCreationContext context );

      public virtual void OnInspectResponse( IHttpResponseInspectionContext context ) { }

      public virtual IEnumerator OnBeforeTranslate( IHttpTranslationContext context ) => null;

      public IEnumerator Translate( ITranslationContext context )
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
            OnCreateRequest( httpContext );
            if( httpContext.Request == null )
            {
               httpContext.Fail( "No request object was provided by the translator.", null );
               yield break;
            }

            var client = new XUnityWebClient();
            response = client.Send( httpContext.Request );
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

         httpContext.Response = response;
         OnInspectResponse( httpContext );

         // failure
         if( response.Error != null )
         {
            httpContext.Fail( "Error occurred while retrieving translation.", response.Error );
            yield break;
         }

         // failure
         if( response.Data == null )
         {
            httpContext.Fail( "Error occurred while retrieving translation. Nothing was returned.", null );
            yield break;
         }

         try
         {
            // attempt to extract translation from data
            OnExtractTranslation( httpContext );
         }
         catch( Exception e )
         {
            httpContext.Fail( "Error occurred while retrieving translation.", e );
         }
      }
   }
}
