using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Lec
{
   internal abstract class UnmanagedTranslationLibrary : IDisposable
   {
      protected UnmanagedLibraryLoader Loader = new UnmanagedLibraryLoader();

      public UnmanagedTranslationLibrary( string libraryPath )
      {
         var ok = Initialize( libraryPath );

         if( !ok ) throw new Exception( "Could not load library." );
      }

      public bool Initialize( string libraryPath )
      {
         if( !File.Exists( libraryPath ) ) return false;

         return Loader.LoadLibrary( libraryPath ) && OnInitialize( libraryPath );
      }

      protected abstract bool OnInitialize( string libraryPath );

      public abstract string Translate( string untranslatedText );

      public static IntPtr ConvertStringToNative( string managedString, int codepage )
      {
         var encoding = Encoding.GetEncoding( codepage );
         var buffer = encoding.GetBytes( managedString );
         IntPtr nativeUtf8 = Marshal.AllocHGlobal( buffer.Length + 1 );
         Marshal.Copy( buffer, 0, nativeUtf8, buffer.Length );
         Marshal.WriteByte( nativeUtf8, buffer.Length, 0 );
         return nativeUtf8;
      }

      public static string ConvertNativeToString( IntPtr nativeUtf8 )
      {
         int len = 0;
         while( Marshal.ReadByte( nativeUtf8, len ) != 0 ) ++len;
         byte[] buffer = new byte[ len ];
         Marshal.Copy( nativeUtf8, buffer, 0, buffer.Length );
         return Encoding.UTF8.GetString( buffer );
      }

      protected virtual void Dispose( bool disposing )
      {
         if( disposing )
         {
            Loader.Dispose();
         }
      }

      public void Dispose()
      {
         Dispose( true );
      }
   }
}
