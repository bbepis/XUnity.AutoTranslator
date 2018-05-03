using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public static class KnownEndpoints
   {
      public static readonly KnownEndpoint GoogleTranslate = new GoogleTranslateEndpoint();
      public static readonly KnownEndpoint BaiduTranslate = new BaiduTranslateEndpoint();

      public static KnownEndpoint FindEndpoint( string identifier )
      {
         if( string.IsNullOrEmpty( identifier ) ) return null;

         switch( identifier )
         {
            case KnownEndpointNames.GoogleTranslate:
               return GoogleTranslate;
            case KnownEndpointNames.BaiduTranslate:
               return BaiduTranslate;
            default:
               return new DefaultEndpoint( identifier );
         }
      }
   }
}
