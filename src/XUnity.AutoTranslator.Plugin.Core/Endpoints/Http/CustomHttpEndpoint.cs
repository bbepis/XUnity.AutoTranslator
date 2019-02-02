using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   internal class CustomHttpEndpoint : HttpEndpoint
   {
      private static readonly string ServicePointTemplateUrl = "{0}?from={1}&to={2}&text={3}";
      private string _endpoint;
      private string _friendlyName;

      public CustomHttpEndpoint()
      {
         _friendlyName = "Custom";
      }

      public override string Id => "Custom";

      public override string FriendlyName => "Custom";

      public override void Initialize( IConfiguration configuration, ServiceEndpointConfiguration servicePoints )
      {
         _endpoint = configuration.Preferences[ "Custom" ][ "Url" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( _endpoint ) ) throw new ArgumentException( "The custom endpoint requires a url which has not been provided." );

         var uri = new Uri( _endpoint );
         servicePoints.EnableHttps( uri.Host );

         _friendlyName += " (" + uri.Host + ")";
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         translated = result;
         return true;
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( ServicePointTemplateUrl, _endpoint, from, to, WWW.EscapeURL( untranslatedText ) );
      }
   }
}
