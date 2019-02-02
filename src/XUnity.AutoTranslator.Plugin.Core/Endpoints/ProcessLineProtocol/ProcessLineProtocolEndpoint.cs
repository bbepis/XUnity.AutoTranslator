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

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ProcessLineProtocol
{

   public abstract class ProcessLineProtocolEndpoint : ITranslateEndpoint, IDisposable
   {
      private bool _disposed = false;
      private Process _process;
      protected string _exePath;
      protected string _arguments;

      public abstract string Id { get; }

      public abstract string FriendlyName { get; }

      public abstract void Initialize( IConfiguration configuration, ServiceEndpointConfiguration servicePoints );

      public int MaxConcurrency => 1;

      protected void Process_OutputDataReceived( object sender, DataReceivedEventArgs e )
      {
      }

      public IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action<string, Exception> failure )
      {
         var _result = new StreamReaderResult();
         try
         {
            try
            {
               ThreadPool.QueueUserWorkItem( state =>
               {
                  if( _process == null )
                  {
                     _process = new Process();
                     _process.StartInfo.FileName = _exePath;
                     _process.StartInfo.Arguments = _arguments;
                     _process.EnableRaisingEvents = false;
                     _process.StartInfo.UseShellExecute = false;
                     _process.StartInfo.RedirectStandardInput = true;
                     _process.StartInfo.RedirectStandardOutput = true;
                     _process.StartInfo.RedirectStandardError = true;
                     _process.OutputDataReceived += Process_OutputDataReceived;
                     _process.Start();

                     // wait a second...
                     _process.WaitForExit( 2500 );
                  }

                  if( _process.HasExited )
                  {
                     _result.SetCompleted( null, "The translation process exited. Likely due to invalid path to installation." );
                     return;
                  }

                  var payload = Convert.ToBase64String( Encoding.UTF8.GetBytes( untranslatedText ) );
                  _process.StandardInput.WriteLine( payload );

                  var returnedPayload = _process.StandardOutput.ReadLine();
                  var returnedLine = Encoding.UTF8.GetString( Convert.FromBase64String( returnedPayload ) );

                  _result.SetCompleted( returnedLine, string.IsNullOrEmpty( returnedLine ) ? "Nothing was returned." : null );
               } );
            }
            catch( Exception e )
            {
               failure( "Error occurred while retrieving translation.", e );
               yield break;
            }

            // yield-wait for completion
            if( Features.SupportsCustomYieldInstruction )
            {
               yield return _result;
            }
            else
            {
               while( !_result.IsCompleted )
               {
                  yield return new WaitForSeconds( 0.2f );
               }
            }

            try
            {
               if( _result.Succeeded && _result.Result != null )
               {
                  success( _result.Result );
               }
               else
               {
                  failure( "Error occurred while retrieving translation." + Environment.NewLine + _result.Error, null );
               }
            }
            catch( Exception e )
            {
               failure( "Error occurred while retrieving translation.", e );
            }
         }
         finally
         {
            _result = null;
         }
      }

      public void OnUpdate()
      {
      }

      public class StreamReaderResult : CustomYieldInstructionShim
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
