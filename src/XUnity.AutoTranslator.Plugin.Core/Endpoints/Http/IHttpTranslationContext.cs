using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   public interface IHttpTranslationContext
   {
      string UntranslatedText { get; }
      string SourceLanguage { get; }
      string DestinationLanguage { get; }

      void Fail( string reason, Exception exception );
   }
}
