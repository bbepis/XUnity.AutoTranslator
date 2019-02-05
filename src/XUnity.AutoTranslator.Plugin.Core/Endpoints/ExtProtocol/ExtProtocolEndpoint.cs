using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Shim;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.AutoTranslator.Plugin.ExtProtocol;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol
{

   public abstract class ExtProtocolEndpoint : ITranslateEndpoint, IDisposable
   {
      private bool _disposed = false;
      private Process _process;
      protected string _exePath;
      protected string _arguments;

      public abstract string Id { get; }

      public abstract string FriendlyName { get; }

      public abstract void Initialize( IInitializationContext context );

      public int MaxConcurrency => 1;

      public IEnumerator Translate( ITranslationContext context )
      {
         var result = new StreamReaderResult();
         try
         {
            ThreadPool.QueueUserWorkItem( state =>
            {
               try
               {
                  if( _process == null )
                  {
                     _process = new Process();
                     _process.StartInfo.FileName = _exePath;
                     _process.StartInfo.Arguments = _arguments;
                     _process.EnableRaisingEvents = false;
                     _process.StartInfo.UseShellExecute = false;
                     _process.StartInfo.CreateNoWindow = true;
                     _process.StartInfo.RedirectStandardInput = true;
                     _process.StartInfo.RedirectStandardOutput = true;
                     _process.StartInfo.RedirectStandardError = true;
                     _process.Start();

                     // wait a second...
                     _process.WaitForExit( 2500 );
                  }

                  if( _process.HasExited )
                  {
                     result.SetCompleted( null, "The translation process exited. Likely due to invalid path to installation." );
                     return;
                  }

                  var request = new TranslationRequest
                  {
                     Id = Guid.NewGuid(),
                     SourceLanguage = context.SourceLanguage,
                     DestinationLanguage = context.DestinationLanguage,
                     UntranslatedText = context.UntranslatedText
                  };
                  var payload = ExtProtocolConvert.Encode( request );
                  _process.StandardInput.WriteLine( payload );

                  var returnedPayload = _process.StandardOutput.ReadLine();
                  var response = ExtProtocolConvert.Decode( returnedPayload );

                  HandleProtocolMessage( result, response );
               }
               catch( Exception e )
               {
                  result.SetCompleted( null, e.Message );
               }
            } );

            // yield-wait for completion
            if( Features.SupportsCustomYieldInstruction )
            {
               yield return result;
            }
            else
            {
               while( !result.IsCompleted )
               {
                  yield return new WaitForSeconds( 0.2f );
               }
            }

            try
            {
               if( result.Succeeded )
               {
                  context.Complete( result.Result );
               }
               else
               {
                  context.Fail( "Error occurred while retrieving translation." + Environment.NewLine + result.Error, null );
               }
            }
            catch( Exception e )
            {
               context.Fail( "Error occurred while retrieving translation.", e );
            }
         }
         finally
         {
            result = null;
         }
      }

      private static void HandleProtocolMessage( StreamReaderResult result, ProtocolMessage message )
      {
         switch( message )
         {
            case TranslationResponse translationResponse:
               HandleTranslationResponse( result, translationResponse );
               break;
            case TranslationError translationResponse:
               HandleTranslationError( result, translationResponse );
               break;
            default:
               result.SetCompleted( null, "Received invalid response." );
               break;
         }
      }

      private static void HandleTranslationResponse( StreamReaderResult result, TranslationResponse message )
      {
         result.SetCompleted( message.TranslatedText, null );
      }

      private static void HandleTranslationError( StreamReaderResult result, TranslationError message )
      {
         result.SetCompleted( null, message.Reason );
      }

      private class StreamReaderResult : CustomYieldInstructionShim
      {
         public void SetCompleted( string result, string error )
         {
            IsCompleted = true;
            Result = result;
            Error = error;
         }

         public override bool keepWaiting => !IsCompleted;

         public string Result { get; set; }

         public string Error { get; set; }

         public bool IsCompleted { get; private set; } = false;

         public bool Succeeded => Error == null;
      }

      #region IDisposable Support

      protected virtual void Dispose( bool disposing )
      {
         if( !_disposed )
         {
            if( disposing )
            {
               if(_process != null )
               {
                  _process.Kill();
                  _process.Dispose();
               }
            }

            _disposed = true;
         }
      }

      // This code added to correctly implement the disposable pattern.
      public void Dispose()
      {
         Dispose( true );
      }

      #endregion
   }
}
