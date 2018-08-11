using System;
using System.Collections;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
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

      /// <summary>
      /// Gets a bool indicating if the plugin is capable of distinguishing between the untranslated text
      /// on a line per line basis.
      /// </summary>
      bool SupportsLineSplitting { get; }
   }
}
