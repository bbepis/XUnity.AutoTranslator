using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// A protocol message containing configuration for the external
   /// endpoint.
   /// </summary>
   public class ConfigurationMessage : ProtocolMessage
   {
      /// <summary>
      /// Gets the type used by the message.
      /// </summary>
      public static readonly string Type = "4";

      /// <summary>
      /// Gets or sets the config.
      /// </summary>
      public string Config { get; set; }

      internal override void Decode( TextReader reader )
      {
         Id = new Guid( reader.ReadLine() );
         Config = reader.ReadToEnd();
      }

      internal override void Encode( TextWriter writer )
      {
         writer.WriteLine( Id.ToString() );
         writer.Write( Config );
      }
   }
}
