using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   /// <summary>
   /// Interface used in the context of the WwwEndpoint.
   /// </summary>
   public interface IWwwTranslationContext
   {
      /// <summary>
      /// Gets the untranslated text.
      /// </summary>
      string UntranslatedText { get; }

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
   }
}
