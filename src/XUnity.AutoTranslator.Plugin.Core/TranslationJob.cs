using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class TranslationJob
   {
      public TranslationJob( TranslationKey key )
      {
         Key = key;

         Components = new List<object>();
         OriginalSources = new HashSet<object>();
         Contexts = new HashSet<TranslationContext>();
      }

      public HashSet<TranslationContext> Contexts { get; private set; }

      public List<object> Components { get; private set; }

      public HashSet<object> OriginalSources { get; private set; }

      public TranslationKey Key { get; private set; }

      public string TranslatedText { get; set; }

      public TranslationJobState State { get; set; }

      public bool AnyComponentsStillHasOriginalUntranslatedTextOrContextual()
      {
         if( Components.Count == 0 || Contexts.Count > 0 ) return true; // we do not know

         for( int i = Components.Count - 1 ; i >= 0 ; i-- )
         {
            var component = Components[ i ];
            try
            {
               var text = component.GetText().TrimIfConfigured(); 
               if( text == Key.OriginalText )
               {
                  return true;
               }
            }
            catch( NullReferenceException )
            {
               // might fail if compoent is no longer associated to game
               Components.RemoveAt( i );
            }
         }

         return false;
      }

      public void Associate( TranslationContext context )
      {
         if( context != null )
         {
            Contexts.Add( context );
            context.Jobs.Add( this );
         }
      }
   }

   public enum TranslationJobState
   {
      RunningOrQueued,
      Succeeded,
      Failed
   }

   public class TranslationContext
   {
      public TranslationContext( object component, ParserResult result )
      {
         Jobs = new HashSet<TranslationJob>();
         Component = component;
         Result = result;
      }

      public ParserResult Result { get; private set; }

      public HashSet<TranslationJob> Jobs { get; private set; }

      public object Component { get; private set; }
   }
}
