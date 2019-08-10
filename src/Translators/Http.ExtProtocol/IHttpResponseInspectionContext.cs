namespace Http.ExtProtocol
{
   /// <summary>
   /// Interface used in the context of inspecting a web response object.
   /// </summary>
   public interface IHttpResponseInspectionContext : IHttpTranslationContext
   {
      /// <summary>
      /// Gets the web request.
      /// </summary>
      XUnityWebRequest Request { get; }

      /// <summary>
      /// Gets the web response.
      /// </summary>
      XUnityWebResponse Response { get; }
   }
}
