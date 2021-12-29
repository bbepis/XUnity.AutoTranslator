using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.AutoTranslator.Plugin.ExtProtocol;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol
{
   /// <summary>
   /// An implementation of ITranslateEndpoint that simplifies implementing
   /// the interface based on an external program.
   /// </summary>
   public abstract class ExtProtocolEndpoint : IMonoBehaviour_Update, ITranslateEndpoint, IDisposable
   {
      private static readonly Random Rng = new Random();

      private readonly Dictionary<Guid, ProtocolTransactionHandle> _transactionHandles = new Dictionary<Guid, ProtocolTransactionHandle>();
      private readonly object _sync = new object();

      private bool _disposed = false;
      private Process _process;
      private Thread _thread;
      private bool _startedThread;
      private bool _initializing;
      private bool _failed;
      private float _lastRequestTimestamp;
      private string _gameRoot;

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
      public virtual int MaxTranslationsPerRequest => 1;

      /// <summary>
      /// Gets the path to the executable that should be communicated with.
      /// </summary>
      protected string ExecutablePath { get; set; }

      /// <summary>
      /// Gets the arguments that should be supplied to the executable.
      /// </summary>
      protected string Arguments { get; set; }

      /// <summary>
      /// Gets the minimum delay to wait before sending a new request.
      /// </summary>
      protected float MinDelay { get; set; }

      /// <summary>
      /// Gets the maximum delay to wait before sending a new request.
      /// </summary>
      protected float MaxDelay { get; set; }

      /// <summary>
      /// Gets the name of the configuration section this translator uses, if any.
      /// </summary>
      protected virtual string ConfigurationSectionName => null;

      /// <summary>
      /// Gets or sets the config the external endpoing will be provided in its Initialize method.
      /// </summary>
      protected string ConfigForExternalProcess { get; set; }

      /// <summary>
      /// Called during initialization. Use this to initialize plugin or throw exception if impossible.
      ///
      /// Must set the ExecutablePath and optionally the Arguments property.
      /// </summary>
      public virtual void Initialize( IInitializationContext context )
      {
         _gameRoot = Paths.GameRoot;

         string exePath = null;
         if( ConfigurationSectionName != null )
         {
            exePath = context.GetOrCreateSetting<string>( ConfigurationSectionName, "ExecutableLocation", null );
         }

         if( string.IsNullOrEmpty( exePath ) )
         {
            exePath = Path.Combine( context.TranslatorDirectory, @"FullNET\Common.ExtProtocol.Executor.exe" );
         }

         var fileExists = File.Exists( exePath );
         if( !fileExists ) throw new EndpointInitializationException( $"Could not find any executable at '{exePath}'" );

         ExecutablePath = exePath;
      }

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
               string fullPath = ExecutablePath;
               if( !Path.IsPathRooted( fullPath ) )
               {
                  try
                  {
                     fullPath = Path.Combine( _gameRoot, ExecutablePath );
                  }
                  catch
                  {
                     fullPath = Path.Combine( Environment.CurrentDirectory, ExecutablePath );
                  }
               }

               _process = new Process();
               _process.StartInfo.FileName = fullPath;
               _process.StartInfo.Arguments = Arguments;
               _process.StartInfo.WorkingDirectory = new FileInfo( ExecutablePath ).Directory.FullName;
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

            SendConfigurationIfRequired();
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

            XuaLogger.AutoTranslator.Error( e, "Error occurred while reading standard output from external process." );
         }
      }

      /// <summary>
      /// Update callback.
      /// </summary>
      public virtual void Update()
      {
         if( TimeSupport.Time.frameCount % 30 == 0 )
         {
            lock( _sync )
            {
               var time = TimeSupport.Time.realtimeSinceStartup;

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
                     kvp.Value.SetCompleted( null, "Request timed out.", StatusCode.Unknown );
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

      private void SendConfigurationIfRequired()
      {
         var configForExternalProcess = ConfigForExternalProcess;
         if( configForExternalProcess != null )
         {
            var id = Guid.NewGuid();

            try
            {
               var request = new ConfigurationMessage
               {
                  Id = id,
                  Config = configForExternalProcess,
               };
               var payload = ExtProtocolConvert.Encode( request );

               _process.StandardInput.WriteLine( payload );
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, $"An error occurred while sending configuration to external process for '{GetType().Name}'." );
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

         while( _initializing && !_failed )
         {
            var instruction = CoroutineHelper.CreateWaitForSecondsRealtime( 0.2f );
            if( instruction != null )
            {
               yield return instruction;
            }
            else
            {
               yield return null;
            }
         }

         if( _failed ) context.Fail( "External process failed." );

         var totalDelay = (float)( Rng.Next( (int)( ( MaxDelay - MinDelay ) * 1000 ) ) + ( MinDelay * 1000 ) ) / 1000;
         var timeSinceLast = TimeSupport.Time.realtimeSinceStartup - _lastRequestTimestamp;
         if( timeSinceLast < totalDelay )
         {
            var remainingDelay = totalDelay - timeSinceLast;

            var instruction = CoroutineHelper.CreateWaitForSecondsRealtime( remainingDelay );
            if( instruction != null )
            {
               yield return instruction;
            }
            else
            {
               float start = TimeSupport.Time.realtimeSinceStartup;
               var end = start + remainingDelay;
               while( TimeSupport.Time.realtimeSinceStartup < end )
               {
                  yield return null;
               }
            }
         }
         _lastRequestTimestamp = TimeSupport.Time.realtimeSinceStartup;

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
               UntranslatedTextInfos = context.UntranslatedTextInfos.Select( x => x.ToTransmittable() ).ToArray()
            };
            var payload = ExtProtocolConvert.Encode( request );

            _process.StandardInput.WriteLine( payload );
         }
         catch( Exception e )
         {
            result.SetCompleted( null, e.Message, StatusCode.Unknown );
         }

         // yield-wait for completion
         var iterator = result.GetSupportedEnumerator();
         while( iterator.MoveNext() ) yield return iterator.Current;

         if( !result.Succeeded ) context.Fail( "Error occurred while retrieving translation. " + result.Error );

         context.Complete( result.Results );
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
               result.SetCompleted( message.TranslatedTexts, null, StatusCode.OK );
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
               result.SetCompleted( null, message.Reason, message.FailureCode );
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
                  //_process.Kill();
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
