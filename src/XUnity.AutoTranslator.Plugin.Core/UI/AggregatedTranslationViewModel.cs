using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class AggregatedTranslationViewModel
   {
      private List<Translation> _translations;
      private TranslationAggregatorViewModel _parent;
      private float? _started;

      public AggregatedTranslationViewModel( TranslationAggregatorViewModel parent, List<Translation> translations )
      {
         _parent = parent;
         _translations = translations;
         AggregatedTranslations = parent.AvailableTranslators.Select(
            x => new IndividualTranslatorTranslationViewModel(
               x,
               new IndividualTranslationViewModel(
                  x,
                  translations.Select( y => new Translation( y.OriginalText, null ) ).ToList() ) ) ).ToList();
      }

      public List<IndividualTranslatorTranslationViewModel> AggregatedTranslations { get; set; }

      public IEnumerable<string> DefaultTranslations => _translations.Select( x => x.TranslatedText );

      public IEnumerable<string> OriginalTexts => _translations.Select( x => x.OriginalText );

      public void CopyDefaultTranslationToClipboard()
      {
         ClipboardHelper.CopyToClipboard( DefaultTranslations, short.MaxValue );
      }

      public void CopyOriginalTextToClipboard()
      {
         ClipboardHelper.CopyToClipboard( OriginalTexts, short.MaxValue );
      }

      public void Update()
      {
         if( _parent.IsShown )
         {
            if( _parent.Manager.OngoingTranslations == 0 && _parent.Manager.UnstartedTranslations == 0 )
            {
               if( _started.HasValue )
               {
                  var timeSince = Time.realtimeSinceStartup - _started.Value;
                  if( timeSince > 1.0f )
                  {
                     foreach( var additionTranslation in AggregatedTranslations )
                     {
                        additionTranslation.Translation.StartTranslations();
                     }
                  }
               }
               else
               {
                  _started = Time.realtimeSinceStartup;
               }
            }

            foreach( var additionTranslation in AggregatedTranslations )
            {
               additionTranslation.Translation.CheckCompleted();
            }
         }
      }
   }
}
