using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Harmony;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class BingTranslateLegitimateEndpoint : KnownHttpEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from={0}&to={1}";
      private static readonly string RequestTemplate = "[{{\"Text\":\"{0}\"}}]";
      private static readonly System.Random RandomNumbers = new System.Random();

      private static readonly string[] Accepts = new string[] { "application/json" };
      private static readonly string[] ContentTypes = new string[] { "application/json" };

      private static readonly string Accept = Accepts[ RandomNumbers.Next( Accepts.Length ) ];
      private static readonly string ContentType = ContentTypes[ RandomNumbers.Next( ContentTypes.Length ) ];

      private string _key;

      public BingTranslateLegitimateEndpoint( string key )
      {
         if( string.IsNullOrEmpty( key ) ) throw new ArgumentException( "The BingTranslateLegitimate endpoint requires an API key which has not been provided.", nameof( key ) );

         _key = key;

         // Configure service points / service point manager
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( "api.cognitive.microsofttranslator.com" );
         SetupServicePoints( "https://api.cognitive.microsofttranslator.com" );
      }

      public override bool SupportsLineSplitting => false;

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
         if( Accept != null )
         {
            headers[ HttpRequestHeader.Accept ] = Accept;
         }
         if( ContentType != null )
         {
            headers[ HttpRequestHeader.ContentType ] = ContentType;
         }

         headers[ "Ocp-Apim-Subscription-Key" ] = _key;
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            var arr = JSON.Parse( result );

            var token = arr.AsArray[ 0 ]?.AsObject[ "translations" ]?.AsArray[ 0 ]?.AsObject[ "text" ]?.ToString();
            token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

            translated = token;

            var success = !string.IsNullOrEmpty( translated );
            return success;
         }
         catch
         {
            translated = null;
            return false;
         }
      }

      public override string GetRequestObject( string untranslatedText, string from, string to )
      {
         return string.Format( RequestTemplate, untranslatedText.EscapeJson() );

      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( HttpsServicePointTemplateUrl, from, to );
      }
   }
}
