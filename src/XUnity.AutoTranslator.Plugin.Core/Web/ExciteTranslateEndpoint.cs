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
   public class ExciteTranslateEndpoint : KnownWwwEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://www.excite.co.jp/world/?wb_lp={0}{1}&before={2}";

      public ExciteTranslateEndpoint()
      {
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( "www.excite.co.jp", "excite.co.jp" );
      }

      public override void ApplyHeaders( Dictionary<string, string> headers )
      {
         headers[ "User-Agent" ] = Settings.GetUserAgent( "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53" );
         headers[ "Accept" ] = "text/html";
         //headers[ "Accept-Charset" ] = "UTF-8";
         //headers[ "DNT" ] = "1";
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            string extracted = result.GetBetween( "class=\"inputText\">", "</p>" );
            translated = RestSharp.Contrib.HttpUtility.HtmlDecode( extracted ?? string.Empty );
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
         return string.Format( HttpsServicePointTemplateUrl, from.ToUpper(), to.ToUpper(), WWW.EscapeURL( untranslatedText ) );
      }
   }
}
