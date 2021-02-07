using Common.ExtProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Http.ExtProtocol
{
   public abstract class ExtHttpEndpoint : IExtTranslateEndpoint
   {
      /// <summary>
      /// Callback for when the endpoint is initialized.
      /// </summary>
      /// <param name="config"></param>
      public virtual void Initialize( string config)
      {

      }

      /// <summary>
      /// Callback that can be overwritten that is called before any requests are sent out.
      /// </summary>
      /// <param name="context"></param>
      /// <returns></returns>
      public virtual Task OnBeforeTranslate( IHttpTranslationContext context ) => null;

      /// <summary>
      /// Callback that must be overwritten to create the request object.
      /// </summary>
      /// <param name="context"></param>
      public abstract void OnCreateRequest( IHttpRequestCreationContext context );

      /// <summary>
      /// Callback that can be overwritten to inspect the response before extracting the text.
      /// </summary>
      /// <param name="context"></param>
      public virtual void OnInspectResponse( IHttpResponseInspectionContext context ) { }

      /// <summary>
      /// Callback that must overwritten to extract the text from the web response.
      /// </summary>
      /// <param name="context"></param>
      public abstract void OnExtractTranslation( IHttpTranslationExtractionContext context );

      /// <summary>
      /// Attempt to translated the provided untranslated text. Will be used in a "coroutine",
      /// so it can be implemented in an asynchronous fashion.
      /// </summary>
      public async Task Translate( ITranslationContext context )
      {
         var httpContext = new HttpTranslationContext( context );

         // allow implementer of HttpEndpoint to do anything before starting translation
         await OnBeforeTranslate( httpContext );

         // prepare request
         OnCreateRequest( httpContext );
         if( httpContext.Request == null ) httpContext.Fail( "No request object was provided by the translator." );

         // execute request
         var client = new XUnityWebClient();
         var response = await client.SendAsync( httpContext.Request );

         httpContext.Response = response;
         OnInspectResponse( httpContext );

         // failure
         if( response.Error != null ) httpContext.Fail( "Error occurred while retrieving translation.", response.Error );

         // failure
         if( response.Data == null ) httpContext.Fail( "Error occurred while retrieving translation. Nothing was returned." );

         // extract text
         OnExtractTranslation( httpContext );
      }
   }
}
