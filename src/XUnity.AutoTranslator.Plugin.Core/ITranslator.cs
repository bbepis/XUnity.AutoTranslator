using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   interface ITranslator
   {
      TranslationResult Translate( string untranslatedText ); // or just use callback?
   }

   class TranslationResult : IEnumerator
   {
      public event Action<string> Completed;
      public event Action<string> Error;

      internal bool IsCompleted { get; private set; }

      public string TranslatedText { get; private set; }

      public void SetCompleted( string translatedText )
      {
         TranslatedText = translatedText;
         IsCompleted = true;

         Completed?.Invoke( translatedText );
      }

      public void SetError()
      {
         IsCompleted = true;

         Error?.Invoke( "Oh no!" );
      }

      public object Current => null;

      public bool MoveNext()
      {
         return !IsCompleted;
      }

      public void Reset()
      {
      }
   }
}
