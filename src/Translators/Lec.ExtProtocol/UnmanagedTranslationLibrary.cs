using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lec.ExtProtocol
{
   internal abstract class UnmanagedTranslationLibrary : IDisposable
   {
      protected UnmanagedLibraryLoader Loader = new UnmanagedLibraryLoader();

      public UnmanagedTranslationLibrary( string libraryPath )
      {
         if( !File.Exists( libraryPath ) ) throw new FileNotFoundException( "Could not find file.", libraryPath );

         Loader.LoadLibrary( libraryPath );
         Initialize( libraryPath );
      }

      protected abstract void Initialize( string libraryPath );

      public abstract string Translate( string untranslatedText );

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
