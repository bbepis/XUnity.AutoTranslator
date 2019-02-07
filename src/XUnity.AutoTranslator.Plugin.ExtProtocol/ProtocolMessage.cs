using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Base-class for all protocol messages.
   /// </summary>
   public abstract class ProtocolMessage
   {
      /// <summary>
      /// Gets or sets the Id of the message.
      /// </summary>
      public Guid Id { get; set; }

      internal abstract void Decode( TextReader reader );

      internal abstract void Encode( TextWriter writer );
   }
}
