namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   public interface IWwwRequestCreationContext : IWwwTranslationContext
   {
      void Complete( WwwRequestInfo requestInfo );
   }
}
