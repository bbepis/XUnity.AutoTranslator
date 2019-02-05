using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   public interface IHttpResponseInspectionContext : IHttpTranslationContext
   {
      XUnityWebRequest Request { get; }

      XUnityWebResponse Response { get; }
   }
}
