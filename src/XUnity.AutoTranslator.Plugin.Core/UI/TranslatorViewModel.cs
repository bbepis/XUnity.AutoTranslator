using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class TranslatorViewModel
   {
      public TranslatorViewModel( TranslationEndpointManager endpoint )
      {
         Endpoint = endpoint;
         IsEnabled = false; // TODO: initialize from configuration...
      }

      public TranslationEndpointManager Endpoint { get; set; }

      public bool IsEnabled { get; set; }
   }
}
