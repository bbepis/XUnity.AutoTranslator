using System;
using System.Runtime.InteropServices;

namespace XUnity.AutoTranslator.Plugin.Lec
{
   public sealed class UnmanagedLibraryLoader : IDisposable
   {
      private IntPtr _libraryPointer;
      private bool _disposed = false;

      public bool LoadLibrary( string path )
      {
         _libraryPointer = Kernel32.LoadLibrary( path );

         return _libraryPointer != IntPtr.Zero;
      }

      public TDelegate LoadFunction<TDelegate>( string name )
      {
         if( _libraryPointer == IntPtr.Zero ) throw new InvalidOperationException( $"Could not load the function {name} because no library has been loaded!" );

         var addr = Kernel32.GetProcAddress( _libraryPointer, name );
         if( addr == IntPtr.Zero ) throw new InvalidOperationException( $"Could not find the function pointer {name}!" );

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
