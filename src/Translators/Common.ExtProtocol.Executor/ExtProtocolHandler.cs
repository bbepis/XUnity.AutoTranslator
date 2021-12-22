using Common.ExtProtocol;
using System;
using System.Collections.Generic;
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

      //public async Task Test()
      //{
      //   var context = new TranslationContext(
      //      new[]
      //      {
      //         new TransmittableUntranslatedTextInfo
      //         {
      //            UntranslatedText = "おはよう",
      //            ContextAfter = new string[0],
      //            ContextBefore = new string[0]
      //         }
      //      },
      //      "ja",
      //      "en" );
      //   try
      //   {
      //      await Endpoint.Translate( context );
      //   }
      //   catch( Exception e )
      //   {
      //      context.FailWithoutThrowing( "An error occurred in the pipeline.", e );
      //   }
      //}

      public async Task RunAsync()
      {
         try
         {
            var usedIds = new HashSet<Guid>();

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

                  var message = ExtProtocolConvert.Decode( receivedPayload );
                  if( message == null ) return;
                  if( !usedIds.Add( message.Id ) ) return;

                  if( message is ConfigurationMessage configurationMessage )
                  {
                     Endpoint.Initialize( configurationMessage.Config );
                  }
                  else if( message is TranslationRequest translationMessage )
                  {
                     var context = new TranslationContext( translationMessage.UntranslatedTextInfos, translationMessage.SourceLanguage, translationMessage.DestinationLanguage );
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
                           Id = translationMessage.Id,
                           Reason = errorMessage
                        };
                     }
                     else
                     {
                        response = new TranslationResponse
                        {
                           Id = translationMessage.Id,
                           TranslatedTexts = context.TranslatedTexts
                        };
                     }

                     var translatedPayload = ExtProtocolConvert.Encode( response );
                     writer.WriteLine( translatedPayload );
                  }
                  else
                  {
                     return;
                  }
               }
            }
         }
         finally
         {
            if( Endpoint is IDisposable disposable )
            {
               disposable.Dispose();
            }
         }
      }
   }
}
