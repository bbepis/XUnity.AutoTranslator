using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class DefaultEndpoint : KnownEndpoint
   {
      private static ServicePoint ServicePoint;
      private static readonly string ServicePointTemplateUrl = "{0}?from={1}&to={2}&text={3}";

      public DefaultEndpoint( string endpoint )
         : base( endpoint )
      {
      }

      public override void ApplyHeaders( Dictionary<string, string> headers )
      {
      }

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
      }

      public override void ConfigureServicePointManager()
      {
         try
         {
            ServicePoint = ServicePointManager.FindServicePoint( new Uri( Identifier ) );
            ServicePoint.ConnectionLimit = Settings.MaxConcurrentTranslations;
         }
         catch
         {
         }
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         translated = result;
         return true;
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( ServicePointTemplateUrl, Identifier, from, to, WWW.EscapeURL( untranslatedText ) );
      }
   }
}
