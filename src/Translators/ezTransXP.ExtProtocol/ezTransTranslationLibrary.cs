using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ezTransXP.ExtProtocol
{
   class ezTransTranslationLibrary : IDisposable
   {
      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate bool J2K_InitializeEx( string username, string path );

      [UnmanagedFunctionPointer( CallingConvention.Cdecl, CharSet = CharSet.Unicode )]
      private delegate IntPtr J2K_TranslateMMNTW( int data0, [MarshalAs( UnmanagedType.LPWStr )]string toTranslate );

      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate void J2K_StopTranslation();

      private string libraryPath;

      protected UnmanagedLibraryLoader Loader = new UnmanagedLibraryLoader();

      private J2K_InitializeEx _init;
      private J2K_TranslateMMNTW _translate;
      private J2K_StopTranslation _end;

      public ezTransTranslationLibrary( string libraryPath )
      {
         this.libraryPath = libraryPath;

         if( !File.Exists( libraryPath ) ) throw new FileNotFoundException( "Could not find file.", libraryPath );

         Loader.LoadLibrary( libraryPath );
         Initialize( libraryPath );
      }

      public bool InitEx(string username, string datpath)
      {
         return _init( username, datpath );
      }

      public string Translate( string toTranslate )
      {
         try
         {
            string result = Marshal.PtrToStringAuto( _translate( 0, toTranslate ) );
            return result;
         }
         catch( Exception e )
         {
            //Failed to Translate
            return toTranslate;
         }
      }

      protected void Initialize( string libraryPath )
      {
         try
         {
            _end = Loader.LoadFunction<J2K_StopTranslation>( "J2K_StopTranslation" );
            _translate = Loader.LoadFunction<J2K_TranslateMMNTW>( "J2K_TranslateMMNTW" );
            _init = Loader.LoadFunction<J2K_InitializeEx>( "J2K_InitializeEx" );
         }
         catch( Exception e )
         {
            throw new Exception( $"Could not load functions from ezTrans library '{libraryPath}'.", e );
         }
      }

      protected virtual void Dispose( bool disposing )
      {
         if( disposing )
         {
            _end();
            Loader.Dispose();
         }
      }

      public void Dispose()
      {
         Dispose( true );
      }


   }
}
