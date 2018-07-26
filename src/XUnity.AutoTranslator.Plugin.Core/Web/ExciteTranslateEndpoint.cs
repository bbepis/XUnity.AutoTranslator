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
    public class ExciteTranslateEndpoint : KnownEndpoint
    {
        private static ServicePoint ServicePoint;

        private static readonly string HttpsServicePointTemplateUrl = "https://www.excite.co.jp/world/?wb_lp={0}{1}&before={2}";

        // Author: Johnny Cee (https://stackoverflow.com/questions/10709821/find-text-in-string-with-c-sharp)
        private static string getBetween(string strSource, string strStart, string strEnd)
        {
            const int kNotFound = -1;

            var startIdx = strSource.IndexOf(strStart);
            if (startIdx != kNotFound)
            {
                startIdx += strStart.Length;
                var endIdx = strSource.IndexOf(strEnd, startIdx);
                if (endIdx > startIdx)
                {
                    return strSource.Substring(startIdx, endIdx - startIdx);
                }
            }
            return String.Empty;
        }

        public ExciteTranslateEndpoint()
           : base(KnownEndpointNames.ExciteTranslate)
        {

        }

        public override void ApplyHeaders(Dictionary<string, string> headers)
        {
            headers["User-Agent"] = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53";
            headers["Accept"] = "text/html";
            headers["Accept-Charset"] = "UTF-8";
            headers["DNT"] = "1";
        }

        public override void ApplyHeaders(WebHeaderCollection headers)
        {
        }

        public override void ConfigureServicePointManager()
        {
            try
            {

                ServicePoint = ServicePointManager.FindServicePoint(new Uri("https://www.excite.co.jp"));
                ServicePoint.ConnectionLimit = Settings.MaxConcurrentTranslations;
            }
            catch
            {
            }
        }

        public override bool TryExtractTranslated(string result, out string translated)
        {
            try
            {                
                String extracted = getBetween(result, "class=\"inputText\">", "</p>");
                if (String.IsNullOrEmpty(extracted))
                {
                    translated = null;
                    return false;
                }
                else
                {
                    translated = RestSharp.Contrib.HttpUtility.HtmlDecode(extracted);
                    return true;
                }
            }
            catch
            {
                translated = null;
                return false;
            }
        }

        public override string GetServiceUrl(string untranslatedText, string from, string to)
        {
            return string.Format(HttpsServicePointTemplateUrl, from.ToUpper(), to.ToUpper(), WWW.EscapeURL(untranslatedText));
        }
    }
}
