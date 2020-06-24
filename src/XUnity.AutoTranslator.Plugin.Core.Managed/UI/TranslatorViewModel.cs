using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class TranslatorViewModel
   {
      private bool _isEnabled;

      public TranslatorViewModel( TranslationEndpointManager endpoint )
      {
         Endpoint = endpoint;
         IsEnabled = Settings.EnabledTranslators.Contains( endpoint.Endpoint.Id );
      }

      public TranslationEndpointManager Endpoint { get; set; }

      public bool IsEnabled
      {
         get { return _isEnabled; }
         set
         {
            if( _isEnabled != value )
            {
               _isEnabled = value;

               if( _isEnabled )
               {
                  Settings.AddTranslator( Endpoint.Endpoint.Id );
               }
               else
               {
                  Settings.RemoveTranslator( Endpoint.Endpoint.Id );
               }
            }
         }
      }
   }
}
