using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   public class WwwTranslationContext
   {
      private readonly TranslationContext _context;

      internal WwwTranslationContext( TranslationContext context )
      {
         _context = context;
      }

      public string UntranslatedText => _context.UntranslatedText;
      public string SourceLanguage => _context.SourceLanguage;
      public string DestinationLanguage => _context.DestinationLanguage;
      public string ResultData { get; internal set; }

      internal string ServiceUrl { get; private set; }
      internal string Data { get; private set; }
      internal Dictionary<string, string> Headers { get; private set; }

      public void SetServiceUrl( string serviceUrl )
      {
         if( string.IsNullOrEmpty( serviceUrl ) ) throw new ArgumentNullException( nameof( serviceUrl ), "Received empty service url from translator." );

         ServiceUrl = serviceUrl;
      }

      public void SetRequestObject( string data )
      {
         Data = data;
      }

      public void SetHeaders( Dictionary<string, string> headers )
      {
         Headers = headers;
      }

      public void Complete( string translatedText )
      {
         _context.Complete( translatedText );
      }

      public void Fail( string reason, Exception exception )
      {
         _context.Fail( reason, exception );
      }
   }
}
