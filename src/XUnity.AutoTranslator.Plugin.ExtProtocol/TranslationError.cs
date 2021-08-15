using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Protocol message returned to a translator if an error occured.
   /// </summary>
   public class TranslationError : ProtocolMessage
   {
      /// <summary>
      /// Gets the type used by the message.
      /// </summary>
      public static readonly string Type = "3";

      /// <summary>
      /// Gets or sets the reason for the failure.
      /// </summary>
      public string Reason { get; set; }

      /// <summary>
      /// Gets or sets the status code used to determine the reason for failure.
      /// </summary>
      public StatusCode FailureCode { get; set; }

      internal override void Decode( TextReader reader )
      {
         Id = new Guid( reader.ReadLine() );
         FailureCode = (StatusCode)int.Parse( reader.ReadLine() );
         Reason = reader.ReadToEnd();
      }

      internal override void Encode( TextWriter writer )
      {
         writer.WriteLine( Id.ToString() );
         writer.WriteLine( (int)FailureCode );
         writer.Write( Reason );
      }
   }
}
