using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public static class KnownEndpoints
   {
      public static readonly KnownEndpoint GoogleTranslateLegacy = new GoogleTranslateEndpoint();
      public static readonly KnownEndpoint GoogleTranslateHack = new GoogleTranslateHackEndpoint();
      public static readonly KnownEndpoint BaiduTranslate = new BaiduTranslateEndpoint();
      public static readonly KnownEndpoint YandexTranslate = new YandexTranslateEndpoint();
      public static readonly KnownEndpoint WatsonTranslate = new WatsonTranslateEndpoint();
      public static readonly KnownEndpoint ExciteTranslate = new ExciteTranslateEndpoint();

      public static KnownEndpoint FindEndpoint( string identifier )
      {
         if( string.IsNullOrEmpty( identifier ) ) return null;

         switch( identifier )
         {
            case KnownEndpointNames.GoogleTranslate:
               return GoogleTranslateLegacy;
            case KnownEndpointNames.GoogleTranslateHack:
               return GoogleTranslateHack;
            case KnownEndpointNames.BaiduTranslate:
               return BaiduTranslate;
            case KnownEndpointNames.YandexTranslate:
               return YandexTranslate;
            case KnownEndpointNames.WatsonTranslate:
               return WatsonTranslate;
            case KnownEndpointNames.ExciteTranslate:
               return ExciteTranslate;
            default:
               return new DefaultEndpoint( identifier );
         }
      }
   }
}
