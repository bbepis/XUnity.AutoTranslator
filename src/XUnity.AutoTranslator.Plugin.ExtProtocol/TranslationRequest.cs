using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   public class TranslationRequest : ProtocolMessage
   {
      public static readonly string Type = "1";

      public Guid Id { get; set; }

      public string SourceLanguage { get; set; }

      public string DestinationLanguage { get; set; }

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
