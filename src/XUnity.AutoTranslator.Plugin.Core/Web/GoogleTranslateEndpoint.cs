using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class GoogleTranslateEndpoint : KnownEndpoint
   {
      //private static readonly string CertificateIssuer = "CN=Google Internet Authority G3, O=Google Trust Services, C=US";
      private static ServicePoint ServicePoint;
      private static readonly string HttpServicePointTemplateUrl = "http://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}";
      private static readonly string HttpsServicePointTemplateUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}";

      public GoogleTranslateEndpoint()
         : base( KnownEndpointNames.GoogleTranslate )
      {

      }

      public override void ApplyHeaders( Dictionary<string, string> headers )
      {
         headers[ "User-Agent" ] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36";
         headers[ "Accept" ] = "*/*";
         headers[ "Accept-Charset" ] = "UTF-8";
      }

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
         headers[ HttpRequestHeader.UserAgent ] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36";
         headers[ HttpRequestHeader.Accept ] = "*/*";
         headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";
      }

      public override void ConfigureServicePointManager()
      {
         try
         {
            //ServicePointManager.ServerCertificateValidationCallback += ( sender, certificate, chain, sslPolicyErrors ) =>
            //{
            //   return certificate.Issuer == CertificateIssuer;
            //};

            ServicePoint = ServicePointManager.FindServicePoint( new Uri( "http://translate.googleapis.com" ) );
            ServicePoint.ConnectionLimit = 5;
            
         }
         catch
         {
         }
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            var arr = JSON.Parse( result );
            var lineBuilder = new StringBuilder( result.Length );

            foreach( JSONNode entry in arr.AsArray[ 0 ].AsArray )
            {
               var token = entry.AsArray[ 0 ].ToString();
               token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

               if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );

               lineBuilder.Append( token );
            }

            translated = lineBuilder.ToString();
            return true;
         }
         catch
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( Settings.EnableSSL ? HttpsServicePointTemplateUrl : HttpServicePointTemplateUrl, from, to, WWW.EscapeURL( untranslatedText ) );
      }
   }
}
