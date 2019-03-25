using System;
using System.Runtime.InteropServices;

namespace Lec.ExtProtocol
{
   public sealed class UnmanagedLibraryLoader : IDisposable
   {
      private IntPtr _libraryPointer;
      private bool _disposed = false;

      public void LoadLibrary( string path )
      {
         _libraryPointer = Kernel32.LoadLibrary( path );
         if( _libraryPointer == IntPtr.Zero ) throw new Exception( $"Could not load the unmanaged library '{path}'." );
      }

      public TDelegate LoadFunction<TDelegate>( string name )
      {
         var addr = Kernel32.GetProcAddress( _libraryPointer, name );
         if( addr == IntPtr.Zero ) throw new Exception( $"Could not find the function pointer for '{name}'." );

         return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer( addr, typeof( TDelegate ) );
      }

      #region IDisposable Support

      void Dispose( bool disposing )
      {
         if( !_disposed )
         {
            Kernel32.FreeLibrary( _libraryPointer );

            _disposed = true;
         }
      }

      ~UnmanagedLibraryLoader()
      {
         Dispose( false );
      }
      
      public void Dispose()
      {
         Dispose( true );
         GC.SuppressFinalize( this );
      }

      #endregion
   }
}
