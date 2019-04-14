using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{

   class AggregatedTranslationViewModel
   {
      private List<Translation> _translations;
      private TranslationAggregatorViewModel _parent;
      private float _started;

      public AggregatedTranslationViewModel( TranslationAggregatorViewModel parent, List<Translation> translations )
      {
         _started = Time.realtimeSinceStartup;
         _parent = parent;
         _translations = translations;
         AggregatedTranslations = parent.Endpoints.Select(
            x => new IndividualTranslatorTranslationViewModel(
               x,
               new IndividualTranslationViewModel(
                  x,
                  translations.Select( y => new Translation( y.OriginalText, null ) ).ToList() ) ) ).ToList();
      }

      public List<IndividualTranslatorTranslationViewModel> AggregatedTranslations { get; set; }

      public IEnumerable<string> DefaultTranslations => _translations.Select( x => x.TranslatedText );

      public IEnumerable<string> OriginalTexts => _translations.Select( x => x.OriginalText );

      public void Update()
      {
         if( _parent.IsShown )
         {
            var timeSince = Time.realtimeSinceStartup - _started;
            if( timeSince > 1.0f )
            {
               foreach( var additionTranslation in AggregatedTranslations )
               {
                  additionTranslation.Translation.Update();
               }
            }
         }
      }
   }
}
