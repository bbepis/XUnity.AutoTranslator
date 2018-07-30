using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public interface IKnownEndpoint
   {
      /// <summary>
      /// Attempt to translated the provided untranslated text. Will be used in a "coroutine", so it can be implemented
      /// in an async fashion.
      /// </summary>
      IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action failure );
      
      /// <summary>
      /// Gets a boolean indicating if we are allowed to call "Translate".
      /// </summary>
      bool IsBusy { get; }

      /// <summary>
      /// Called before plugin shutdown and can return true to prevent plugin shutdown, if the plugin
      /// can provide a secondary strategy for translation.
      /// </summary>
      /// <returns></returns>
      bool ShouldGetSecondChanceAfterFailure();

      /// <summary>
      /// "Update" game loop method.
      /// </summary>
      void OnUpdate();
   }
}
