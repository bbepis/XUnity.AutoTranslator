using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public static class KnownEndpoints
   {
      public static IKnownEndpoint FindEndpoint( string identifier )
      {
         if( string.IsNullOrEmpty( identifier ) ) return null;

         switch( identifier )
         {
            case KnownEndpointNames.GoogleTranslate:
            case KnownEndpointNames.GoogleTranslateHack:
               return new GoogleTranslateEndpoint();
               //return new GoogleTranslateHackEndpoint();
            case KnownEndpointNames.GoogleTranslateLegitimate:
               return new GoogleTranslateLegitimateEndpoint( Settings.GoogleAPIKey );
            case KnownEndpointNames.BaiduTranslate:
               return new BaiduTranslateEndpoint( Settings.BaiduAppId, Settings.BaiduAppSecret );
            case KnownEndpointNames.YandexTranslate:
               return new YandexTranslateEndpoint( Settings.YandexAPIKey );
            case KnownEndpointNames.WatsonTranslate:
               return new WatsonTranslateEndpoint( Settings.WatsonAPIUrl, Settings.WatsonAPIUsername, Settings.WatsonAPIPassword );
            case KnownEndpointNames.ExciteTranslate:
               return new ExciteTranslateEndpoint();
            //case KnownEndpointNames.BingTranslate:
            //   return new BingTranslateEndpoint();
            case KnownEndpointNames.BingTranslateLegitimate:
               return new BingTranslateLegitimateEndpoint( Settings.BingOcpApimSubscriptionKey );
            default:
               return new DefaultEndpoint( identifier );
         }
      }
   }
}
