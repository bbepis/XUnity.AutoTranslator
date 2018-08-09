using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Harmony;
using Jurassic;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class GoogleTranslateLegitimateEndpoint : KnownHttpEndpoint
   {
      private static readonly string HttpsServicePointTemplateUrl = "https://translation.googleapis.com/language/translate/v2?key={0}";

      public GoogleTranslateLegitimateEndpoint()
      {
         // Configure service points / service point manager
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( "translation.googleapis.com" );
         SetupServicePoints( "https://translation.googleapis.com" );
      }

      public override bool SupportsLineSplitting => true;

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
      }

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            var obj = JsonUtility.FromJson<GResponse>( result );
            var translations = obj?.data?.translations;
            if( translations != null && translations.Count > 0 )
            {
               translated = translations[ 0 ]?.translatedText;
               return true;
            }
            else
            {
               translated = string.Empty;
               return true;
            }
         }
         catch
         {
            translated = null;
            return false;
         }
      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         return string.Format( HttpsServicePointTemplateUrl, WWW.EscapeURL( Settings.GoogleAPIKey ) );
      }

      public override string GetRequestObject( string untranslatedText, string from, string to )
      {
         return JsonUtility.ToJson( new GRequest
         {
            q = untranslatedText,
            target = to,
            source = from,
            format = "text"
         } );
      }

      public override bool ShouldGetSecondChanceAfterFailure()
      {
         return false;
      }
   }

   [Serializable]
   public class GRequest
   {
      public string q { get; set; }

      public string target { get; set; }

      public string source { get; set; }

      public string format { get; set; }
   }

   public class GResponse
   {
      public GData data { get; set; }
   }

   public class GData
   {
      public List<GTranslation> translations { get; set; }
   }

   public class GTranslation
   {
      public string translatedText { get; set; }
   }

}
