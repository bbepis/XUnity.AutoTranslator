using System;
using System.Runtime.InteropServices;

namespace XUnity.AutoTranslator.Plugin.Lec
{
   internal static class Kernel32
   {
      [DllImport( "kernel32.dll" )]
      public static extern IntPtr LoadLibrary( string dllToLoad );

      [DllImport( "kernel32.dll" )]
      public static extern IntPtr GetProcAddress( IntPtr hModule, string procedureName );

      [DllImport( "kernel32.dll" )]
      public static extern bool FreeLibrary( IntPtr hModule );
   }
}
