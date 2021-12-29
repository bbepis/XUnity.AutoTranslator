using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class IndividualTranslationViewModel
   {
      private string[] _notTranslated = new[] { "Not translated yet." };
      private string[] _requestingTranslation = new[] { "Requesting translation..." };
      private List<Translation> _translations;
      private TranslatorViewModel _translator;
      private bool _hasStartedTranslation;
      private bool _isTranslated;

      public IndividualTranslationViewModel( TranslatorViewModel translator, List<Translation> translations )
      {
         _translator = translator;
         _translations = translations;
      }

      public IEnumerable<string> Translations
      {
         get
         {
            if( _isTranslated )
            {
               return _translations.Select( x => x.TranslatedText );
            }
            else if( _hasStartedTranslation )
            {
               return _requestingTranslation;
            }
            else
            {
               return _notTranslated;
            }
         }
      }

      public void StartTranslations()
      {
         if( _translator.IsEnabled )
         {
            if( !_hasStartedTranslation )
            {
               _hasStartedTranslation = true;

               foreach( var translation in _translations )
               {
                  translation.PerformTranslation( _translator.Endpoint );
               }
            }
         }
      }

      public void CheckCompleted()
      {
         if( _translator.IsEnabled )
         {
            if( !_isTranslated )
            {
               if( _translations.All( x => x.TranslatedText != null ) )
               {
                  _isTranslated = true;
               }
            }
         }
      }

      public void CopyToClipboard()
      {
         if( _isTranslated )
         {
            ClipboardHelper.CopyToClipboard( _translations.Select( x => x.TranslatedText ), short.MaxValue );
         }
      }

      public bool CanCopyToClipboard()
      {
         return _isTranslated;
      }
   }
}
