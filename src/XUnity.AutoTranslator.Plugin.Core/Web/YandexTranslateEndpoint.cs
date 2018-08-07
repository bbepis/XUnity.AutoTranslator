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
   public class YandexTranslateEndpoint : KnownHttpEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://translate.yandex.net/api/v1.5/tr.json/translate?key={3}&text={2}&lang={0}-{1}&format=plain";
      public YandexTranslateEndpoint()
      {
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( "translate.yandex.net" );
      }

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
         headers[ HttpRequestHeader.UserAgent ] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.183 Safari/537.36 Vivaldi/1.96.1147.55";
         headers[ HttpRequestHeader.Accept ] = "*/*";
         headers[ HttpRequestHeader.AcceptCharset ] = "UTF-8";
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {

            var obj = JSON.Parse( result );
            var lineBuilder = new StringBuilder( result.Length );

            var code = obj.AsObject[ "code" ].ToString();

            if( code == "200" )
            {
               var token = obj.AsObject[ "text" ].ToString();
               token = token.Substring( 2, token.Length - 4 ).UnescapeJson();
               if( String.IsNullOrEmpty( token ) )
               {
                  translated = null;
                  return false;
               }

               if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( "\n" );
               lineBuilder.Append( token );

               translated = lineBuilder.ToString();
               return true;
            }
            else
            {
               translated = null;
               return false;
            }
         }
         catch( Exception )
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( HttpsServicePointTemplateUrl, from, to, WWW.EscapeURL( untranslatedText ), Settings.YandexAPIKey );
      }
   }
}
