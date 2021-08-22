namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class IndividualTranslatorTranslationViewModel
   {
      public IndividualTranslatorTranslationViewModel( TranslatorViewModel translator, IndividualTranslationViewModel translation )
      {
         Translator = translator;
         Translation = translation;
      }

      public TranslatorViewModel Translator { get; private set; }

      public IndividualTranslationViewModel Translation { get; private set; }

      public void CopyToClipboard()
      {
         Translation.CopyToClipboard();
      }

      public bool CanCopyToClipboard()
      {
         return Translation.CanCopyToClipboard();
      }
   }
}
