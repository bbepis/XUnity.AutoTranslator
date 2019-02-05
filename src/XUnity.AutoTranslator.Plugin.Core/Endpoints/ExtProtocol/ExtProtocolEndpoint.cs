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

   public abstract class ExtProtocolEndpoint : MonoBehaviour, ITranslateEndpoint, IDisposable
   {
      private readonly Dictionary<Guid, StreamReaderResult> _pendingRequests = new Dictionary<Guid, StreamReaderResult>();
      private readonly object _sync = new object();

      private bool _disposed = false;
      private Process _process;
      private Thread _thread;
      private bool _startedThread;
      private bool _initializing;
      private bool _failed;

      protected string _exePath;
      protected string _arguments;

      public abstract string Id { get; }

      public abstract string FriendlyName { get; }

      public virtual int MaxConcurrency => 1;

      public abstract void Initialize( IInitializationContext context );

      private void EnsureInitialized()
      {
         if( !_startedThread )
         {
            _startedThread = true;
            _initializing = true;

            _thread = new Thread( ReaderLoop );
            _thread.IsBackground = true;
            _thread.Start();
         }
      }

      private void ReaderLoop( object state )
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
               return;
            }
            _initializing = false;

            while( !_disposed )
            {
               var returnedPayload = _process.StandardOutput.ReadLine();
               var response = ExtProtocolConvert.Decode( returnedPayload );
               HandleProtocolMessage( response );
            }
         }
         catch( Exception e )
         {
            _failed = true;
            _initializing = false;

            XuaLogger.Current.Error( e, "Error occurred while reading standard output from external process." );
         }
      }

      void Update()
      {
         if( Time.frameCount % 30 == 0 )
         {
            lock( _sync )
            {
               var time = Time.realtimeSinceStartup;

               List<Guid> idsToRemove = null;
               foreach( var kvp in _pendingRequests )
               {
                  var elapsed = time - kvp.Value.StartTime;
                  if( elapsed > 60 )
                  {
                     if( idsToRemove == null )
                     {
                        idsToRemove = new List<Guid>();
                     }

                     idsToRemove.Add( kvp.Key );
                     kvp.Value.SetCompleted( null, "Request timed out." );
                  }
               }

               if( idsToRemove != null )
               {
                  foreach( var id in idsToRemove )
                  {
                     _pendingRequests.Remove( id );
                  }
               }
            }
         }
      }

      public IEnumerator Translate( ITranslationContext context )
      {
         EnsureInitialized();

         while( _initializing && !_failed )
         {
            yield return new WaitForSeconds( 0.2f );
         }

         if( _failed )
         {
            context.Fail( "Translator failed.", null );
            yield break;
         }

         var result = new StreamReaderResult();
         var id = Guid.NewGuid();

         lock( _sync )
         {
            _pendingRequests[ id ] = result;
         }

         try
         {
            var request = new TranslationRequest
            {
               Id = id,
               SourceLanguage = context.SourceLanguage,
               DestinationLanguage = context.DestinationLanguage,
               UntranslatedText = context.UntranslatedText
            };
            var payload = ExtProtocolConvert.Encode( request );

            _process.StandardInput.WriteLine( payload );
         }
         catch( Exception e )
         {
            result.SetCompleted( null, e.Message );
         }

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

         if( result.Succeeded )
         {
            context.Complete( result.Result );
         }
         else
         {
            context.Fail( "Error occurred while retrieving translation." + Environment.NewLine + result.Error, null );
         }
      }

      private void HandleProtocolMessage( ProtocolMessage message )
      {
         switch( message )
         {
            case TranslationResponse translationResponse:
               HandleTranslationResponse( translationResponse );
               break;
            case TranslationError translationResponse:
               HandleTranslationError( translationResponse );
               break;
            default:
               break;
         }
      }

      private void HandleTranslationResponse( TranslationResponse message )
      {
         lock( _sync )
         {
            if( _pendingRequests.TryGetValue( message.Id, out var result ) )
            {
               result.SetCompleted( message.TranslatedText, null );
               _pendingRequests.Remove( message.Id );
            }
         }
      }

      private void HandleTranslationError( TranslationError message )
      {
         lock( _sync )
         {
            if( _pendingRequests.TryGetValue( message.Id, out var result ) )
            {
               result.SetCompleted( null, message.Reason );
               _pendingRequests.Remove( message.Id );
            }
         }
      }

      private class StreamReaderResult : CustomYieldInstructionShim
      {
         public StreamReaderResult()
         {
            StartTime = Time.realtimeSinceStartup;
         }

         public void SetCompleted( string result, string error )
         {
            IsCompleted = true;
            Result = result;
            Error = error;
         }

         public override bool keepWaiting => !IsCompleted;

         public float StartTime { get; set; }

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
               if( _process != null )
               {
                  _process.Kill();
                  _process.Dispose();
                  _thread.Abort();
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
