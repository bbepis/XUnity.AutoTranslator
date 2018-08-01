using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
               return new GoogleTranslateEndpoint();
            case KnownEndpointNames.GoogleTranslateHack:
               return new GoogleTranslateHackEndpoint();
            case KnownEndpointNames.BaiduTranslate:
               return new BaiduTranslateEndpoint();
            case KnownEndpointNames.YandexTranslate:
               return new YandexTranslateEndpoint();
            case KnownEndpointNames.WatsonTranslate:
               return new WatsonTranslateEndpoint();
            case KnownEndpointNames.ExciteTranslate:
               return new ExciteTranslateEndpoint();
            default:
               return new DefaultEndpoint( identifier );
         }
      }
   }
}
