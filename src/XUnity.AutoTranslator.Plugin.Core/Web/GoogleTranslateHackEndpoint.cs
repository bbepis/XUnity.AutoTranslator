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
   public class GoogleTranslateHackEndpoint : KnownHttpEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://translate.google.com/m?hl=pl&sl={0}&tl={1}&ie=UTF-8&q={2}";

      public GoogleTranslateHackEndpoint()
      {
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( "translate.google.com" );
      }

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
         headers[ HttpRequestHeader.UserAgent ] = Settings.GetUserAgent( "Opera/9.80 (J2ME/MIDP; Opera Mini/5.1.21214/28.2725; U; en) Presto/2.8.119 Version/11.10" );
         headers[ HttpRequestHeader.Accept ] = "*/*";
         headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {

            String extracted = result.GetBetween( "class=\"t0\">", "</div>" );
            translated = RestSharp.Contrib.HttpUtility.HtmlDecode( extracted ?? string.Empty );

            var success = !string.IsNullOrEmpty( translated );
            return success;
         }
         catch
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( HttpsServicePointTemplateUrl, from, to, WWW.EscapeURL( untranslatedText ) );
      }
   }
}
