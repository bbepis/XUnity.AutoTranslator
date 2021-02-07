using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol
{
   /// <summary>
   /// Class used to encode and decode 'Ext' protocol messages.
   /// </summary>
   public static class ExtProtocolConvert
   {
      private static readonly Dictionary<string, Type> IdToType = new Dictionary<string, Type>();
      private static readonly Dictionary<Type, string> TypeToId = new Dictionary<Type, string>();

      static ExtProtocolConvert()
      {
         Register( TranslationRequest.Type, typeof( TranslationRequest ) );
         Register( TranslationResponse.Type, typeof( TranslationResponse ) );
         Register( TranslationError.Type, typeof( TranslationError ) );
         Register( ConfigurationMessage.Type, typeof( ConfigurationMessage ) );
      }

      /// <summary>
      /// Register a message type.
      /// </summary>
      /// <param name="id"></param>
      /// <param name="type"></param>
      public static void Register( string id, Type type )
      {
         IdToType[ id ] = type;
         TypeToId[ type ] = id;
      }

      /// <summary>
      /// Encode a message to a string.
      /// </summary>
      /// <param name="message"></param>
      /// <returns></returns>
      public static string Encode( ProtocolMessage message )
      {
         var writer = new StringWriter();
         var id = TypeToId[ message.GetType() ];
         writer.WriteLine( id );
         message.Encode( writer );
         return Convert.ToBase64String( Encoding.UTF8.GetBytes( writer.ToString() ), Base64FormattingOptions.None );
      }

      /// <summary>
      /// Decode a message from a string.
      /// </summary>
      /// <param name="message"></param>
      /// <returns></returns>
      public static ProtocolMessage Decode( string message )
      {
         var payload = Encoding.UTF8.GetString( Convert.FromBase64String( message ) );
         var reader = new StringReader( payload );
         var id = reader.ReadLine();
         var type = IdToType[ id ];
         var protocolMessage = (ProtocolMessage)Activator.CreateInstance( type );
         protocolMessage.Decode( reader );
         return protocolMessage;
      }
   }
}
