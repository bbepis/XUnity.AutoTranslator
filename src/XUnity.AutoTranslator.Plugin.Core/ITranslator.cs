using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Public API Surface for the Auto Translator Plugin.
   /// </summary>
   public interface ITranslator
   {
      /// <summary>
      /// Queries the plugin to provide a translated text for the untranslated text.
      /// If the translation cannot be found in the cache, it will make a
      /// request to the translator selected by the user.
      /// </summary>
      /// <param name="untranslatedText">The untranslated text to provide a translation for.</param>
      /// <param name="onCompleted">Callback to completion of the translation.</param>
      void TranslateAsync( string untranslatedText, Action<TranslationResult> onCompleted );

      /// <summary>
      /// Queries the plugin to provide a translated text for the untranslated text.
      /// If the translation cannot be found in the cache, the method returns false
      /// and returns null as the untranslated text.
      /// </summary>
      /// <param name="untranslatedText">The untranslated text to provide a translation for.</param>
      /// <param name="translatedText">The translated text.</param>
      /// <returns></returns>
      bool TryTranslate( string untranslatedText, out string translatedText );
   }
}
