using System;
using System.Collections;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   /// <summary>
   /// The interface that must be implemented by a translator.
   /// </summary>
   public interface ITranslateEndpoint
   {
      /// <summary>
      /// Gets the id of the ITranslateEndpoint that is used as a configuration parameter.
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
      /// Gets the maximum number of translations that can be served per translation request.
      /// </summary>
      int MaxTranslationsPerRequest { get; }

      /// <summary>
      /// Called during initialization. Use this to initialize plugin or throw exception if impossible.
      /// </summary>
      void Initialize( IInitializationContext context );

      /// <summary>
      /// Attempt to translated the provided untranslated text. Will be used in a "coroutine",
      /// so it can be implemented in an asynchronous fashion.
      /// </summary>
      IEnumerator Translate( ITranslationContext context );
   }
}
