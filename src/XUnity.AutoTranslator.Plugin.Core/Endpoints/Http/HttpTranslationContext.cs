using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   public class HttpTranslationContext
   {
      private readonly TranslationContext _context;

      internal HttpTranslationContext( TranslationContext context )
      {
         _context = context;
      }

      public string UntranslatedText => _context.UntranslatedText;
      public string SourceLanguage => _context.SourceLanguage;
      public string DestinationLanguage => _context.DestinationLanguage;
      public string ResultData { get; internal set; }

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
