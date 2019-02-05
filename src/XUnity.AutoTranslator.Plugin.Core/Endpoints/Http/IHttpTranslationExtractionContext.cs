using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   public interface IHttpTranslationExtractionContext : IHttpTranslationContext
   {
      XUnityWebRequest Request { get; }

      XUnityWebResponse Response { get; }

      void Complete( string translatedText );
   }
}
