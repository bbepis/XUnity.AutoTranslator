using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.AutoTranslator.Plugin.ExtProtocol;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol
{
   /// <summary>
   /// An implementation of ITranslateEndpoint that simplifies implementing
   /// the interface based on an external program.
   /// </summary>
   public abstract class ExtProtocolEndpoint : MonoBehaviour, ITranslateEndpoint, IDisposable
   {
      private readonly Dictionary<Guid, ProtocolTransactionHandle> _transactionHandles = new Dictionary<Guid, ProtocolTransactionHandle>();
      private readonly object _sync = new object();

      private bool _disposed = false;
      private Process _process;
      private Thread _thread;
      private bool _startedThread;
      private bool _initializing;
      private bool _failed;

      /// <summary>
      /// Gets the id of the ITranslateEndpoint that is used as a configuration parameter.
      /// </summary>
      public abstract string Id { get; }

      /// <summary>
      /// Gets a friendly name that can be displayed to the user representing the plugin.
      /// </summary>
      public abstract string FriendlyName { get; }

      /// <summary>
      /// Gets the maximum concurrency for the endpoint. This specifies how many times "Translate"
      /// can be called before it returns.
      /// </summary>
      public virtual int MaxConcurrency => 1;

      /// <summary>
      /// Gets the maximum number of translations that can be served per translation request.
      /// </summary>
      public int MaxTranslationsPerRequest => 1;

      /// <summary>
      /// Gets the path to the executable that should be communicated with.
      /// </summary>
      protected string ExecutablePath { get; set; }

      /// <summary>
      /// Gets the arguments that should be supplied to the executable.
      /// </summary>
      protected string Arguments { get; set; }

      /// <summary>
      /// Called during initialization. Use this to initialize plugin or throw exception if impossible.
      ///
      /// Must set the ExecutablePath and optionally the Arguments property.
      /// </summary>
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
               _process.StartInfo.FileName = ExecutablePath;
               _process.StartInfo.Arguments = Arguments;
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
               HandleProtocolMessageResponse( response );
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
               foreach( var kvp in _transactionHandles )
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
                     _transactionHandles.Remove( id );
                  }
               }
            }
         }
      }

      /// <summary>
      /// Attempt to translated the provided untranslated text. Will be used in a "coroutine",
      /// so it can be implemented in an asynchronous fashion.
      /// </summary>
      public IEnumerator Translate( ITranslationContext context )
      {
         EnsureInitialized();

         while( _initializing && !_failed ) yield return new WaitForSeconds( 0.2f ); 

         if( _failed ) context.Fail( "Translator failed.", null );

         var result = new ProtocolTransactionHandle();
         var id = Guid.NewGuid();

         lock( _sync )
         {
            _transactionHandles[ id ] = result;
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
         var iterator = result.GetSupportedEnumerator();
         while( iterator.MoveNext() ) yield return iterator.Current;

         if( !result.Succeeded ) context.Fail( "Error occurred while retrieving translation. " + result.Error );

         context.Complete( result.Result );
      }

      private void HandleProtocolMessageResponse( ProtocolMessage message )
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
            if( _transactionHandles.TryGetValue( message.Id, out var result ) )
            {
               result.SetCompleted( message.TranslatedText, null );
               _transactionHandles.Remove( message.Id );
            }
         }
      }

      private void HandleTranslationError( TranslationError message )
      {
         lock( _sync )
         {
            if( _transactionHandles.TryGetValue( message.Id, out var result ) )
            {
               result.SetCompleted( null, message.Reason );
               _transactionHandles.Remove( message.Id );
            }
         }
      }

      #region IDisposable Support

      /// <summary>
      /// Disposes the endpoint and kills the external process.
      /// </summary>
      /// <param name="disposing"></param>
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

      /// <summary>
      /// Disposes the endpoint and kills the external process.
      /// </summary>
      public void Dispose()
      {
         Dispose( true );
      }

      #endregion
   }
}
