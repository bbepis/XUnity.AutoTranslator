using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Protocol message supplied to the external process to indicate
   /// a translation is requested.
   /// </summary>
   public class TranslationRequest : ProtocolMessage
   {
      /// <summary>
      /// Gets the type used by the message.
      /// </summary>
      public static readonly string Type = "1";

      /// <summary>
      /// Gets or sets the source language.
      /// </summary>
      public string SourceLanguage { get; set; }

      /// <summary>
      /// Gets or sets the destination language.
      /// </summary>
      public string DestinationLanguage { get; set; }

      /// <summary>
      /// Gets or sets the untranslated text.
      /// </summary>
      public string UntranslatedText { get; set; }

      internal override void Decode( TextReader reader )
      {
         Id = new Guid( reader.ReadLine() );
         SourceLanguage = reader.ReadLine();
         DestinationLanguage = reader.ReadLine();
         UntranslatedText = reader.ReadToEnd();
      }

      internal override void Encode( TextWriter writer )
      {
         writer.WriteLine( Id.ToString() );
         writer.WriteLine( SourceLanguage );
         writer.WriteLine( DestinationLanguage );
         writer.Write( UntranslatedText );
      }
   }
}
