using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Http
{
   /// <summary>
   /// Interface used in the context of the HttpEndpoint.
   /// </summary>
   public interface IHttpTranslationContext
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
