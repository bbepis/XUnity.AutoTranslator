namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   /// <summary>
   /// Interface used in the context of creating a new web request object.
   /// </summary>
   public interface IWwwRequestCreationContext : IWwwTranslationContext
   {
      /// <summary>
      /// Completes the callback by specifying the created request to use.
      /// </summary>
      /// <param name="requestInfo"></param>
      void Complete( WwwRequestInfo requestInfo );
   }
}
