using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Debugging
{
   public static class Kernel32
   {
      [DllImport( "kernel32.dll", SetLastError = true )]
      public static extern bool AllocConsole();

      [DllImport( "kernel32.dll", SetLastError = false )]
      public static extern bool FreeConsole();

      [DllImport( "kernel32.dll", SetLastError = true )]
      public static extern IntPtr GetStdHandle( int nStdHandle );

      [DllImport( "kernel32.dll", SetLastError = true )]
      public static extern bool SetStdHandle( int nStdHandle, IntPtr hConsoleOutput );

      [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
      public static extern IntPtr CreateFile(
              string fileName,
              int desiredAccess,
              int shareMode,
              IntPtr securityAttributes,
              int creationDisposition,
              int flagsAndAttributes,
              IntPtr templateFile );

      [DllImport( "kernel32.dll", ExactSpelling = true, SetLastError = true )]
      public static extern bool CloseHandle( IntPtr handle );
   }
}
