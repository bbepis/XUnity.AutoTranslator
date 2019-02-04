using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   public class TranslationResponse : ProtocolMessage
   {
      public static readonly string Type = "2";

      public Guid Id { get; set; }

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
