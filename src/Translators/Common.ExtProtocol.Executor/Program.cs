using Common.ExtProtocol;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Http.ExtProtocol.Executor
{
   class Program
   {
      static void Main( string[] args )
      {
         try
         {
            if( args.Length == 0 )
            {
               Console.WriteLine( "This program is automatically run during a game session if an endpoint requiring an external process is selected, and will be automatically stopped afterwards. This program cannot be run independently." );
               Console.WriteLine( "Press any key to exit." );
               Console.ReadKey();
               return;
            }

            var typeDefinitionPayload = args[ 0 ];
            var typeDefinition = Encoding.UTF8.GetString( Convert.FromBase64String( typeDefinitionPayload ) );

            var type = Type.GetType( typeDefinition, false );
            if( type == null )
            {
               return;
            }

            var endpoint = (IExtTranslateEndpoint)Activator.CreateInstance( type );
            var handler = new ExtProtocolHandler( endpoint );
            handler.RunAsync().Wait();
         }
         catch( Exception )
         {
            // "Graceful shutdown"
         }
      }
   }
}
