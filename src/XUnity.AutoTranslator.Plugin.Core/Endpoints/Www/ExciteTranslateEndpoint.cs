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
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   internal class ExciteTranslateEndpoint : WwwEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://www.excite.co.jp/world/?wb_lp={0}{1}&before={2}";

      public ExciteTranslateEndpoint()
      {
      }

      public override string Id => "ExciteTranslate";

      public override string FriendlyName => "Excite Translator";

      public override void Initialize( InitializationContext context )
      {
         context.HttpSecurity.EnableSslFor( "www.excite.co.jp", "excite.co.jp" );
      }

      public override void ApplyHeaders( Dictionary<string, string> headers )
      {
         headers[ "User-Agent" ] = string.IsNullOrEmpty( AutoTranslationState.UserAgent ) ? "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53" : AutoTranslationState.UserAgent;
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
         return string.Format( HttpsServicePointTemplateUrl, from.ToUpper(), to.ToUpper(), WWW.EscapeURL( untranslatedText ) );
      }
   }
}
