namespace Http.ExtProtocol
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
