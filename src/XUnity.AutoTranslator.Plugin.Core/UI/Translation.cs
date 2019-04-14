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
         var response = AutoTranslator.Default.Translate( endpoint, OriginalText );
         response.Completed += Response_Completed;
         response.Error += Response_Error;
      }

      private void Response_Error( string error )
      {
         TranslatedText = error;
      }

      private void Response_Completed( string translatedText )
      {
         TranslatedText = translatedText;
      }
   }
}
