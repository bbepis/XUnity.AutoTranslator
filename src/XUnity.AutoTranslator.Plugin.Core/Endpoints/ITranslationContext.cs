using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   public interface ITranslationContext
   {
      string UntranslatedText { get; }
      string SourceLanguage { get; }
      string DestinationLanguage { get; }

      void Complete( string translatedText );
      void Fail( string reason, Exception exception );
   }
}
