using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   public abstract class ProtocolMessage
   {
      internal abstract void Decode( TextReader reader );

      internal abstract void Encode( TextWriter writer );
   }
}
