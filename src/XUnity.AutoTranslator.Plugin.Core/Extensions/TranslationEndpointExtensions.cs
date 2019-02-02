using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TranslationEndpointExtensions
   {
      public static bool SupportsLineSplitting( this ITranslateEndpoint endpoint )
      {
         return endpoint is GoogleTranslateEndpoint || endpoint is GoogleTranslateLegitimateEndpoint;
      }
   }
}
