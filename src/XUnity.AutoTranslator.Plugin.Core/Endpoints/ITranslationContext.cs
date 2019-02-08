using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   /// <summary>
   /// Interface used in the context of translating a text.
   /// </summary>
   public interface ITranslationContext : ITranslationContextBase
   {
      /// <summary>
      /// Completes the translation by providing the translated text.
      /// </summary>
      /// <param name="translatedText"></param>
      void Complete( string translatedText );
   }

   /// <summary>
   /// Interface used in the context of translating a text.
   /// </summary>
   public interface ITranslationContextBase
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
