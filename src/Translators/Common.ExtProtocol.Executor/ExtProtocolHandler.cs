using Common.ExtProtocol;
using System;
using System.IO;
using System.Threading.Tasks;
using XUnity.AutoTranslator.Plugin.ExtProtocol;

namespace Http.ExtProtocol.Executor
{
   public class ExtProtocolHandler
   {
      public ExtProtocolHandler( IExtTranslateEndpoint endpoint )
      {
         Endpoint = endpoint;
      }

      public IExtTranslateEndpoint Endpoint { get; }

      public async Task RunAsync()
      {
         using( var stdout = Console.OpenStandardOutput() )
         using( var writer = new StreamWriter( stdout ) )
         using( var stdin = Console.OpenStandardInput() )
         using( var reader = new StreamReader( stdin ) )
         {
            writer.AutoFlush = true;

            while( true )
            {
               var receivedPayload = reader.ReadLine();
               if( string.IsNullOrEmpty( receivedPayload ) ) return;

               var message = ExtProtocolConvert.Decode( receivedPayload ) as TranslationRequest;
               if( message == null ) return;

               var context = new TranslationContext( message.UntranslatedTexts, message.SourceLanguage, message.DestinationLanguage );
               try
               {
                  await Endpoint.Translate( context );
               }
               catch( Exception e )
               {
                  context.FailWithoutThrowing( "An error occurred in the pipeline.", e );
               }


               ProtocolMessage response;
               if( !string.IsNullOrEmpty( context.ErrorMessage ) || context.Error != null )
               {
                  string errorMessage = context.ErrorMessage;
                  if( context.Error != null )
                  {
                     if( !string.IsNullOrEmpty( errorMessage ) )
                     {
                        errorMessage += Environment.NewLine;
                     }
                     errorMessage += context.Error.ToString();
                  }

                  response = new TranslationError
                  {
                     Id = message.Id,
                     Reason = errorMessage
                  };
               }
               else
               {
                  response = new TranslationResponse
                  {
                     Id = message.Id,
                     TranslatedTexts = context.TranslatedTexts
                  };
               }

               var translatedPayload = ExtProtocolConvert.Encode( response );
               writer.WriteLine( translatedPayload );
            }
         }
      }
   }
}
