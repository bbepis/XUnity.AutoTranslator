using Common.ExtProtocol;
using System;

namespace Http.ExtProtocol
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
      public string SourceLanguage => _context.SourceLanguage;
      public string DestinationLanguage => _context.DestinationLanguage;
      public XUnityWebResponse Response { get; internal set; }
      public XUnityWebRequest Request { get; internal set; }

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
