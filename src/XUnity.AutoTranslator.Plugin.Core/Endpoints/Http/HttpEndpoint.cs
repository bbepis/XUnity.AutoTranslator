using System;
using System.Collections;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   /// <summary>
   /// An implementation of ITranslateEndpoint that simplifies implementing
   /// the interface based on a web service.
   /// </summary>
   public abstract class HttpEndpoint : ITranslateEndpoint
   {
      /// <summary>
      /// Gets the id of the ITranslateEndpoint that is used as a configuration parameter.
      /// </summary>
      public abstract string Id { get; }

      /// <summary>
      /// Gets a friendly name that can be displayed to the user representing the plugin.
      /// </summary>
      public abstract string FriendlyName { get; }

      /// <summary>
      /// Gets the maximum concurrency for the endpoint. This specifies how many times "Translate"
      /// can be called before it returns.
      /// </summary>
      public virtual int MaxConcurrency => 1;

      /// <summary>
      /// Gets the maximum number of translations that can be served per translation request.
      /// </summary>
      public virtual int MaxTranslationsPerRequest => 1;

      /// <summary>
      /// Called during initialization. Use this to initialize plugin or throw exception if impossible.
      /// </summary>
      public abstract void Initialize( IInitializationContext context );

      /// <summary>
      /// Callback that can be overwritten that is called before any requests are sent out.
      /// </summary>
      /// <param name="context"></param>
      /// <returns></returns>
      public virtual IEnumerator OnBeforeTranslate( IHttpTranslationContext context ) => null;

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
      public IEnumerator Translate( ITranslationContext context )
      {
         var httpContext = new HttpTranslationContext( context );

         // allow implementer of HttpEndpoint to do anything before starting translation
         var setup = OnBeforeTranslate( httpContext );
         if( setup != null )
         {
            while( setup.MoveNext() ) yield return setup.Current;
         }

         // prepare request
         OnCreateRequest( httpContext );
         if( httpContext.Request == null ) httpContext.Fail( "No request object was provided by the translator." );

         // execute request
         var client = new XUnityWebClient();
         var response = client.Send( httpContext.Request );

         // wait for completion
         var iterator = response.GetSupportedEnumerator();
         while( iterator.MoveNext() ) yield return iterator.Current;

         if( response.IsTimedOut ) httpContext.Fail( "Error occurred while retrieving translation. Timeout." );

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
