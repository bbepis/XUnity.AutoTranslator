using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.ExtProtocol;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   /// <summary>
   /// Class representing an untranslated text and the contextual information surrounding it.
   /// </summary>
   public class UntranslatedTextInfo
   {
      internal UntranslatedTextInfo( string untranslatedText )
      {
         UntranslatedText = untranslatedText;
         ContextBefore = new List<string>();
         ContextAfter = new List<string>();
      }

      internal UntranslatedTextInfo( string untranslatedText, List<string> contextBefore, List<string> contextAfter )
      {
         UntranslatedText = untranslatedText;
         ContextBefore = contextBefore;
         ContextAfter = contextAfter;
      }

      /// <summary>
      /// The contextual untranslated text that preceeds the untranslated text.
      /// </summary>
      public List<string> ContextBefore { get; }

      /// <summary>
      /// The untranslated text that should be translated.
      /// </summary>
      public string UntranslatedText { get; internal set; }

      /// <summary>
      /// The contextual untranslated text that comes after the untranslated text.
      /// </summary>
      public List<string> ContextAfter { get; }

      /// <summary>
      /// Converts the untranslated text to a ext protocol compatible format.
      /// </summary>
      /// <returns></returns>
      public TransmittableUntranslatedTextInfo ToTransmittable()
      {
         return new TransmittableUntranslatedTextInfo( ContextBefore.ToArray(), UntranslatedText, ContextAfter.ToArray() );
      }
   }
}
