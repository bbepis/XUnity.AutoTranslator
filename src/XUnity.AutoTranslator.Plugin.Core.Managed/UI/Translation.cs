using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class Translation
   {
      public Translation( string originalText, string translatedText )
      {
         OriginalText = originalText;
         TranslatedText = translatedText;
      }

      public string OriginalText { get; set; }

      public string TranslatedText { get; set; }

      public void PerformTranslation( TranslationEndpointManager endpoint )
      {
         AutoTranslator.Internal.TranslateAsync( endpoint, OriginalText, Response_Completed );
      }

      private void Response_Completed( TranslationResult result )
      {
         if( result.Succeeded )
         {
            TranslatedText = result.TranslatedText;
         }
         else
         {
            TranslatedText = result.ErrorMessage;
         }
      }
   }
}
