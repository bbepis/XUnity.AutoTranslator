using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   public interface IWwwTranslationContext
   {
      string UntranslatedText { get; }
      string SourceLanguage { get; }
      string DestinationLanguage { get; }

      void Fail( string reason, Exception exception );
   }
}
