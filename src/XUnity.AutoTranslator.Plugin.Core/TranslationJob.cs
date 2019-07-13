using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TranslationJob
   {
      public TranslationJob( TranslationEndpointManager endpoint, UntranslatedText key, bool saveResult )
      {
         Endpoint = endpoint;
         Key = key;
         SaveResultGlobally = saveResult;

         Components = new List<KeyAnd<object>>();
         Contexts = new HashSet<ParserTranslationContext>();
         TranslationResults = new HashSet<KeyAnd<TranslationResult>>();
      }

      public bool SaveResultGlobally { get; private set; }

      public TranslationEndpointManager Endpoint { get; private set; }

      public HashSet<ParserTranslationContext> Contexts { get; private set; }

      public List<KeyAnd<object>> Components { get; private set; }

      public HashSet<KeyAnd<TranslationResult>> TranslationResults { get; private set; }

      public UntranslatedText Key { get; private set; }

      public string TranslatedText { get; set; }

      public string ErrorMessage { get; set; }

      public TranslationJobState State { get; set; }

      public TranslationType TranslationType { get; set; }

      public bool IsFullTranslation => ( TranslationType & TranslationType.Full ) == TranslationType.Full;

      public void Associate( UntranslatedText key, object ui, TranslationResult translationResult, ParserTranslationContext context )
      {
         if( context != null )
         {
            Contexts.Add( context );
            context.Jobs.Add( this );

            TranslationType |= TranslationType.Token;
         }
         else
         {
            if( ui != null && !ui.IsSpammingComponent() )
            {
               Components.Add( new KeyAnd<object>( key, ui ) );
            }

            if( translationResult != null )
            {
               TranslationResults.Add( new KeyAnd<TranslationResult>( key, translationResult ) );
            }

            TranslationType |= TranslationType.Full;
         }
      }
   }

   internal class KeyAnd<T>
   {
      public KeyAnd( UntranslatedText key, T item )
      {
         Key = key;
         Item = item;
      }

      public UntranslatedText Key { get; set; }

      public T Item { get; set; }
   }
}
