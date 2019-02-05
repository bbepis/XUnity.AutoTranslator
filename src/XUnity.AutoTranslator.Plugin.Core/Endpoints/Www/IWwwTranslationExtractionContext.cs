namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   public interface IWwwTranslationExtractionContext : IWwwTranslationContext
   {
      string ResponseData { get; }

      void Complete( string translatedText );
   }
}
