using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   /// <summary>
   /// Interface used in the context of extracting a translated text from a web response.
   /// </summary>
   public interface IHttpTranslationExtractionContext : IHttpResponseInspectionContext
   {
      /// <summary>
      /// Completes the translation by providing the translated text.
      /// </summary>
      /// <param name="translatedText"></param>
      void Complete( string translatedText );
   }
}
