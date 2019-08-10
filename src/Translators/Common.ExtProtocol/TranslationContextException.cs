using System;

namespace Common.ExtProtocol
{
   [Serializable]
   internal class TranslationContextException : Exception
   {
      public TranslationContextException() { }
      public TranslationContextException( string message ) : base( message ) { }
      public TranslationContextException( string message, Exception inner ) : base( message, inner ) { }
      protected TranslationContextException(
       System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
   }
}
