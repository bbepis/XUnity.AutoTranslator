using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   public class TranslationError : ProtocolMessage
   {
      public static readonly string Type = "3";

      public Guid Id { get; set; }

      public string Reason { get; set; }

      internal override void Decode( TextReader reader )
      {
         Id = new Guid( reader.ReadLine() );
         Reason = reader.ReadToEnd();
      }

      internal override void Encode( TextWriter writer )
      {
         writer.WriteLine( Id.ToString() );
         writer.Write( Reason );
      }
   }
}
