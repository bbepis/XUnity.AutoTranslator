using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Protocol message returned to a translator when a translation completed.
   /// </summary>
   public class TranslationResponse : ProtocolMessage
   {
      /// <summary>
      /// Gets the type used by the message.
      /// </summary>
      public static readonly string Type = "2";

      /// <summary>
      /// Gets or sets the translated text.
      /// </summary>
      public string TranslatedText { get; set; }

      internal override void Decode( TextReader reader )
      {
         Id = new Guid( reader.ReadLine() );
         TranslatedText = reader.ReadToEnd();
      }

      internal override void Encode( TextWriter writer )
      {
         writer.WriteLine( Id.ToString() );
         writer.Write( TranslatedText );
      }
   }
}
