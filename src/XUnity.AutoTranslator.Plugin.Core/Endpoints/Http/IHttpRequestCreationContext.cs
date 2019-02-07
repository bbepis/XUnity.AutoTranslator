using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   /// <summary>
   /// Interface used in the context of creating a new web request object.
   /// </summary>
   public interface IHttpRequestCreationContext : IHttpTranslationContext
   {
      /// <summary>
      /// Completes the callback by specifying the created request to use.
      /// </summary>
      /// <param name="request"></param>
      void Complete( XUnityWebRequest request );
   }
}
