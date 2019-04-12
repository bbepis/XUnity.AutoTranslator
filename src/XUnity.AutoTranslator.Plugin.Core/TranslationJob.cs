using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TranslationJob
   {
      public TranslationJob( TranslationEndpointManager endpoint, TranslationKey key, bool saveResult )
      {
         Endpoint = endpoint;
         Key = key;
         SaveResult = saveResult;

         Components = new List<object>();
         Contexts = new HashSet<ParserTranslationContext>();
      }

      public bool SaveResult { get; private set; }

      public TranslationEndpointManager Endpoint { get; private set; }

      public HashSet<ParserTranslationContext> Contexts { get; private set; }

      public List<object> Components { get; private set; }

      public TranslationKey Key { get; private set; }

      public string TranslatedText { get; set; }

      public TranslationJobState State { get; set; }

      public void Associate( object ui, ParserTranslationContext context )
      {
         if( context != null )
         {
            Contexts.Add( context );
            context.Jobs.Add( this );
         }
         else
         {
            if( ui != null && !ui.IsSpammingComponent() )
            {
               Components.Add( ui );
            }
         }
      }
   }
}
