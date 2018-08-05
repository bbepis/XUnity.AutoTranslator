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

      public bool AnyComponentsStillHasOriginalUntranslatedText()
      {
         if( Components.Count == 0 ) return true; // we do not know

         foreach( var component in Components )
         {
            var text = component.GetText().Trim();
            if( text == Key.OriginalText )
            {
               return true;
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
