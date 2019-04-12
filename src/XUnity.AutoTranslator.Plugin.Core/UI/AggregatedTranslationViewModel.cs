using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class AggregatedTranslationViewModel
   {
      private string _originalText;
      private string _mainTranslation;
      private Dictionary<TranslatorViewModel, string> _additionalTranslations;

      public AggregatedTranslationViewModel( TranslationAggregatorViewModel parent, TextTranslationInfo info )
      {
         _originalText = info.OriginalText;
         _mainTranslation = info.TranslatedText;
         _additionalTranslations = parent.Endpoints.ToDictionary( x => x, x => (string)null );
      }

      public void StartAdditionnalTranslations()
      {
         foreach( var additionalTranslation in _additionalTranslations.Where( x => x.Key.IsEnabled ) )
         {
            var vm = additionalTranslation.Key;
            var translation = additionalTranslation.Value;
            if( translation == null )
            {

            }
         }
      }
   }

   class TranslationAggregatorViewModel
   {
      private LinkedList<AggregatedTranslationViewModel> _translations;
      private LinkedListNode<AggregatedTranslationViewModel> _current;
      private List<TranslatorViewModel> _endpoints;

      public TranslationAggregatorViewModel( IEnumerable<TranslationEndpointManager> endpoints )
      {
         _translations = new LinkedList<AggregatedTranslationViewModel>();
         _endpoints = endpoints
            .Where( x => x.Error == null )
            .Select( x => new TranslatorViewModel( x ) )
            .ToList();
      }

      public List<TranslatorViewModel> Endpoints => _endpoints;

      public void OnNewTranslationAdded( TextTranslationInfo info )
      {
         var vm = new AggregatedTranslationViewModel( this, info );

         var previousLast = _translations.Last;

         _translations.AddLast( vm );
         if( _current == null )
         {
            _current = _translations.Last;
         }
         else
         {
            if( _current == previousLast )
            {
               _current = _translations.Last;
            }
         }
      }
   }

   class TranslatorViewModel
   {
      private TranslationEndpointManager _endpoint;

      public TranslatorViewModel( TranslationEndpointManager endpoint )
      {
         _endpoint = endpoint;
         IsEnabled = false; // initialize from configuration...
      }

      public bool IsEnabled { get; set; }
   }
}
