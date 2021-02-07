using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   internal class HttpTranslationContext : IHttpTranslationContext, IHttpRequestCreationContext, IHttpResponseInspectionContext, IHttpTranslationExtractionContext
   {
      private readonly ITranslationContext _context;

      internal HttpTranslationContext( ITranslationContext context )
      {
         _context = context;
      }

      public string UntranslatedText => _context.UntranslatedText;
      public string[] UntranslatedTexts => _context.UntranslatedTexts;
      public UntranslatedTextInfo UntranslatedTextInfo => _context.UntranslatedTextInfo;
      public UntranslatedTextInfo[] UntranslatedTextInfos => _context.UntranslatedTextInfos;

      public string SourceLanguage => _context.SourceLanguage;
      public string DestinationLanguage => _context.DestinationLanguage;
      public XUnityWebResponse Response { get; internal set; }
      public XUnityWebRequest Request { get; internal set; }
      public object UserState
      {
         get => _context.UserState;
         set => _context.UserState = value;
      }


      public void Fail( string reason, Exception exception )
      {
         _context.Fail( reason, exception );
      }

      public void Fail( string reason )
      {
         _context.Fail( reason );
      }


      void IHttpRequestCreationContext.Complete( XUnityWebRequest request )
      {
         Request = request;
      }

      void IHttpTranslationExtractionContext.Complete( string translatedText )
      {
         _context.Complete( translatedText );
      }

      void IHttpTranslationExtractionContext.Complete( string[] translatedTexts )
      {
         _context.Complete( translatedTexts );
      }
   }
}
