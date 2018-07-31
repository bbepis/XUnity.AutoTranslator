using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class DefaultEndpoint : KnownHttpEndpoint
   {
      private static readonly string ServicePointTemplateUrl = "{0}?from={1}&to={2}&text={3}";
      private string _endpoint;

      public DefaultEndpoint( string endpoint )
      {
         _endpoint = endpoint;
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( new Uri( _endpoint ).Host );
      }

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
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
