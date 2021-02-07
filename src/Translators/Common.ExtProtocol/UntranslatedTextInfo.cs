using XUnity.AutoTranslator.Plugin.ExtProtocol;

namespace Common.ExtProtocol
{
   public class UntranslatedTextInfo
   {
      public UntranslatedTextInfo( TransmittableUntranslatedTextInfo untranslatedTextInfo )
      {
         ContextBefore = untranslatedTextInfo.ContextBefore;
         UntranslatedText = untranslatedTextInfo.UntranslatedText;
         ContextAfter = untranslatedTextInfo.ContextAfter;
      }

      public UntranslatedTextInfo( string[] contextBefore, string untranslatedText, string[] contextAfter )
      {
         ContextBefore = contextBefore;
         UntranslatedText = untranslatedText;
         ContextAfter = contextAfter;
      }

      /// <summary>
      /// The contextual untranslated text that preceeds the untranslated text.
      /// </summary>
      public string[] ContextBefore { get; }

      /// <summary>
      /// The untranslated text that should be translated.
      /// </summary>
      public string UntranslatedText { get; }

      /// <summary>
      /// The contextual untranslated text that comes after the untranslated text.
      /// </summary>
      public string[] ContextAfter { get; }
   }
}
