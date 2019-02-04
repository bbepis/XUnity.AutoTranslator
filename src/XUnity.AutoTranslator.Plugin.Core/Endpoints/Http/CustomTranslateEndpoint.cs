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

      public override void Initialize( InitializationContext context )
      {
         _endpoint = context.Config.Preferences[ "Custom" ][ "Url" ].GetOrDefault( "" );
         if( string.IsNullOrEmpty( _endpoint ) ) throw new ArgumentException( "The custom endpoint requires a url which has not been provided." );

         var uri = new Uri( _endpoint );
         context.HttpSecurity.EnableSslFor( uri.Host );

         _friendlyName += " (" + uri.Host + ")";
      }

      public override XUnityWebRequest CreateTranslationRequest( HttpTranslationContext context )
      {
         var request = new XUnityWebRequest(
            string.Format(
               ServicePointTemplateUrl,
               _endpoint,
               context.SourceLanguage,
               context.DestinationLanguage,
               WWW.EscapeURL( context.UntranslatedText ) ) );

         return request;
      }

      public override void ExtractTranslatedText( HttpTranslationContext context )
      {
         context.Complete( context.ResultData );
      }
   }
}
