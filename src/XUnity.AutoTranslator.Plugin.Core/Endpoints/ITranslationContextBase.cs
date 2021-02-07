using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   /// <summary>
   /// Interface used in the context of translating a text.
   /// </summary>
   public interface ITranslationContextBase
   {
      /// <summary>
      /// Gets the first untranslated text.
      /// </summary>
      string UntranslatedText { get; }

      /// <summary>
      /// Gets all the untranslated texts. The number of texts
      /// in array cannot be greater than the MaxTranslationsPerRequest
      /// in the ITranslateEndpoint interface.
      /// </summary>
      string[] UntranslatedTexts { get; }

      /// <summary>
      /// Gets the first untranslated text info.
      /// </summary>
      UntranslatedTextInfo UntranslatedTextInfo { get; }

      /// <summary>
      /// Gets all the untranslated texts. The number of texts
      /// in array cannot be greater than the MaxTranslationsPerRequest
      /// in the ITranslateEndpoint interface.
      /// </summary>
      UntranslatedTextInfo[] UntranslatedTextInfos { get; }

      /// <summary>
      /// Gets the source language.
      /// </summary>
      string SourceLanguage { get; }

      /// <summary>
      /// Gets the destination language.
      /// </summary>
      string DestinationLanguage { get; }

      /// <summary>
      /// Fails the translation. Immediately throws an exception.
      /// </summary>
      /// <param name="reason"></param>
      /// <param name="exception"></param>
      void Fail( string reason, Exception exception );

      /// <summary>
      /// Fails the translation. Immediately throws an exception.
      /// </summary>
      /// <param name="reason"></param>
      void Fail( string reason );

      /// <summary>
      /// Gets or sets user state that is maintained between various callbacks.
      /// </summary>
      object UserState { get; set; }
   }
}
