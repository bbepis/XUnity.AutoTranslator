using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal class TranslationContext : ITranslationContext
   {
      private Action<string> _complete;
      private Action<string, Exception> _fail;

      public TranslationContext(
         string untranslatedText,
         string sourceLanguage,
         string destinationLanguage,
         Action<string> complete,
         Action<string, Exception> fail )
      {
         UntranslatedText = untranslatedText;
         SourceLanguage = sourceLanguage;
         DestinationLanguage = destinationLanguage;

         _complete = complete;
         _fail = fail;
      }

      public string UntranslatedText { get; }
      public string SourceLanguage { get; }
      public string DestinationLanguage { get; }

      internal bool IsDone { get; private set; }

      public void Complete( string translatedText )
      {
         try
         {
            if( !string.IsNullOrEmpty( translatedText ) )
            {
               _complete( translatedText );
            }
            else
            {
               _fail( "Received empty translation from translator.", null );
            }
         }
         finally
         {
            IsDone = true;
         }
      }

      public void Fail( string reason, Exception exception )
      {
         try
         {
            _fail( reason, exception );
         }
         finally
         {
            IsDone = true;
         }
      }

      internal void FailIfNotCompleted()
      {
         if( !IsDone )
         {
            Fail( "The translation request was not completed before returning from translator.", null );
         }
      }
   }
}
