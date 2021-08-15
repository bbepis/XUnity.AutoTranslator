using System;
using System.IO;
using System.Threading;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal sealed class SafeFileWatcher : IDisposable
   {
      public event Action DirectoryUpdated;

      private int _counter = 0;
      private FileSystemWatcher _watcher;
      private bool _disposed;
      private object _sync = new object();
      private Timer _timer;
      private readonly string _directory;

      public SafeFileWatcher( string directory )
      {
         _directory = directory;
         _timer = new Timer( RaiseEvent, null, Timeout.Infinite, Timeout.Infinite );

         EnableWatcher();
      }

      public void EnableWatcher()
      {
         if( _watcher == null )
         {
            _watcher = new FileSystemWatcher( _directory );
            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Created;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.EnableRaisingEvents = true;
         }
      }

      public void DisableWatcher()
      {
         if( _watcher != null )
         {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
         }
      }

      public void RaiseEvent( object state )
      {
         DirectoryUpdated?.Invoke();
      }

      public void Disable()
      {
         var counter = Interlocked.Increment( ref _counter );
         UpdateRaisingEvents( counter == 0 );
      }

      public void Enable()
      {
         var counter = Interlocked.Decrement( ref _counter );
         UpdateRaisingEvents( counter == 0 );
      }

      private void Watcher_Deleted( object sender, FileSystemEventArgs e )
      {
         _timer.Change( 1000, Timeout.Infinite );
      }

      private void Watcher_Created( object sender, FileSystemEventArgs e )
      {
         var fi = new FileInfo( e.FullPath );
         WaitForFile( fi );

         _timer.Change( 1000, Timeout.Infinite );
      }

      private void Watcher_Changed( object sender, FileSystemEventArgs e )
      {
         _timer.Change( 1000, Timeout.Infinite );
      }

      private void UpdateRaisingEvents( bool enabled )
      {
         lock( _sync )
         {
            if( enabled )
            {
               EnableWatcher();
            }
            else
            {
               DisableWatcher();
            }
         }
      }

      private void WaitForFile( FileInfo file )
      {
         // also time out after a while

         while( IsFileLocked( file ) )
         {
            Thread.Sleep( 100 );
         }
      }

      private bool IsFileLocked( FileInfo file )
      {
         try
         {
            using var stream = file.Open( FileMode.Open, FileAccess.ReadWrite, FileShare.None );
         }
         catch( IOException )
         {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            return true;
         }

         //file is not locked
         return false;
      }

      private void Dispose( bool disposing )
      {
         if( !_disposed )
         {
            if( disposing )
            {
               _watcher?.Dispose();
               _watcher = null;
               _timer.Dispose();
            }

            _disposed = true;
         }
      }

      public void Dispose()
      {
         Dispose( disposing: true );
         GC.SuppressFinalize( this );
      }
   }
}
