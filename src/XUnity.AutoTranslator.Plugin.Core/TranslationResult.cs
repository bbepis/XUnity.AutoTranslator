
namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Class representing the result of a translation.
   /// </summary>
   public class TranslationResult
   {
      internal TranslationResult( string translatedText, string errorMessage )
      {
         TranslatedText = translatedText;
         ErrorMessage = errorMessage;
      }

      /// <summary>
      /// Gets a bool indicating if the translation succeeded.
      /// </summary>
      public bool Succeeded => ErrorMessage == null;

      /// <summary>
      /// Gets the translated text.
      /// </summary>
      public string TranslatedText { get; }

      /// <summary>
      /// Gets the error message if the translation did not succeed.
      /// </summary>
      public string ErrorMessage { get; }
   }
}
