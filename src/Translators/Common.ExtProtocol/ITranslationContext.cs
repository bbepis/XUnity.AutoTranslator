namespace Common.ExtProtocol
{
   /// <summary>
   /// Interface used in the context of translating a text.
   /// </summary>
   public interface ITranslationContext : ITranslationContextBase
   {
      /// <summary>
      /// Completes the translation by providing the translated text.
      /// </summary>
      /// <param name="translatedText"></param>
      void Complete( string translatedText );

      /// <summary>
      /// Completes the translation by providing the translated texts.
      ///
      /// The indices of the translations must match the indices of the
      /// untranslated texts.
      /// </summary>
      /// <param name="translatedTexts"></param>
      void Complete( string[] translatedTexts );
   }
}
