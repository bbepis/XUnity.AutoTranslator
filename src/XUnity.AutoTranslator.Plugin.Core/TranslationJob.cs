using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class TranslationJob
   {
      public TranslationJob( TranslationKeys key )
      {
         Keys = key;

         Components = new List<object>();
      }

      public List<object> Components { get; private set; }
      
      public TranslationKeys Keys { get; private set; }

      public string TranslatedText { get; set; }

      public bool AnyComponentsStillHasOriginalUntranslatedText()
      {
         if( Components.Count == 0 ) return true; // we do not know

         foreach( var component in Components )
         {
            var text = component.GetText().Trim();
            if( text == Keys.OriginalText )
            {
               return true;
            }
         }

         return false;
      }
   }
}
