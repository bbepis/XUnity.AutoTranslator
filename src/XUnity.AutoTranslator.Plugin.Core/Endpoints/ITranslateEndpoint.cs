using System;
using System.Collections;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   public interface ITranslateEndpoint
   {
      /// <summary>
      /// Gets the id of the IKnownEndpoint that is used as a configuration parameter.
      /// </summary>
      string Id { get; }

      /// <summary>
      /// Gets a friendly name that can be displayed to the user representing the plugin.
      /// </summary>
      string FriendlyName { get; }

      /// <summary>
      /// Gets the maximum concurrency for the endpoint. This specifies how many times "Translate"
      /// can be called before it returns.
      /// </summary>
      int MaxConcurrency { get; }

      /// <summary>
      /// Called during initialization. Use this to initialize plugin or throw exception if impossible.
      /// </summary>
      void Initialize( InitializationContext context );

      /// <summary>
      /// Attempt to translated the provided untranslated text. Will be used in a "coroutine", so it can be implemented
      /// in an async fashion.
      /// </summary>
      IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action<string, Exception> failure );
   }
}
