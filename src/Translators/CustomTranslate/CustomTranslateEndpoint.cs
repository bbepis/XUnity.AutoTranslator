using System;
using System.Collections.Generic;
using System.Net;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace CustomTranslate
{
   internal class CustomTranslateEndpoint : HttpEndpoint
   {
      private static readonly string ServicePointTemplateUrl = "{0}?from={1}&to={2}&text={3}";
      private string _endpoint;
      private string _friendlyName;

      public CustomTranslateEndpoint()
      {
         _friendlyName = "Custom";
      }

      public override string Id => "CustomTranslate";

      public override string FriendlyName => _friendlyName;

      public override void Initialize( IInitializationContext context )
      {
         _endpoint = context.GetOrCreateSetting( "Custom", "Url", "" );
         if( string.IsNullOrEmpty( _endpoint ) ) throw new EndpointInitializationException( "The custom endpoint requires a url which has not been provided." );

         var uri = new Uri( _endpoint );
         context.DisableCertificateChecksFor( uri.Host );

         _friendlyName += " (" + uri.Host + ")";
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         var request = new XUnityWebRequest(
            string.Format(
               ServicePointTemplateUrl,
               _endpoint,
               context.SourceLanguage,
               context.DestinationLanguage,
               Uri.EscapeDataString( context.UntranslatedText ) ) );

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         context.Complete( context.Response.Data );
      }
   }
}
