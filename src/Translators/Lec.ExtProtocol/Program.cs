using System;
using System.IO;
using System.Text;
using System.Threading;
using XUnity.AutoTranslator.Plugin.ExtProtocol;

namespace Lec.ExtProtocol
{
   class Program
   {
      static void Main( string[] args )
      {
         try
         {
            if( args.Length == 0 )
            {
               Console.WriteLine( "This program is automatically run during a game session if LEC is selected, and will be automatically stopped afterwards. This program cannot be run independently." );
               Console.WriteLine( "Press any key to exit." );
               Console.ReadKey();
               return;
            }

            var powerTranslatorPathPayload = args[ 0 ];
            var powerTranslatorPath = Encoding.UTF8.GetString( Convert.FromBase64String( powerTranslatorPathPayload ) );
            var dllPath = Path.Combine( powerTranslatorPath, @"Nova\JaEn\EngineDll_je.dll" );

            using( var translator = new LecTranslationLibrary( dllPath ) )
            {
               while( true )
               {
                  using( var stdout = Console.OpenStandardOutput() )
                  using( var writer = new StreamWriter( stdout ) )
                  using( var stdin = Console.OpenStandardInput() )
                  using( var reader = new StreamReader( stdin ) )
                  {
                     var receivedPayload = reader.ReadLine();
                     if( string.IsNullOrEmpty( receivedPayload ) ) return;

                     var message = ExtProtocolConvert.Decode( receivedPayload ) as TranslationRequest;
                     if( message == null ) return;

                     var translatedLine = translator.Translate( message.UntranslatedText );

                     var response = new TranslationResponse
                     {
                        Id = message.Id,
                        TranslatedText = translatedLine
                     };

                     var translatedPayload = ExtProtocolConvert.Encode( response );
                     writer.WriteLine( translatedPayload );
                  }
               }
            }
         }
         catch( Exception )
         {
            // "Graceful shutdown"
         }
      }
   }
}
