using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal static class Kernel32
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

      [DllImport( "kernel32.dll" )]
      public static extern IntPtr SetConsoleOutputCP( uint codepage );

      [DllImport( "kernel32.dll", SetLastError = true )]
      public static extern int WideCharToMultiByte(
          uint codePage,
          uint dwFlags,
          [In, MarshalAs( UnmanagedType.LPArray )] char[] lpWideCharStr,
          int cchWideChar,
          [Out, MarshalAs( UnmanagedType.LPArray )] byte[] lpMultiByteStr,
          int cbMultiByte,
          IntPtr lpDefaultChar,
          IntPtr lpUsedDefaultChar );

      [DllImport( "kernel32.dll", SetLastError = true )]
      public static extern int MultiByteToWideChar(
          uint codePage,
          uint dwFlags,
          [In, MarshalAs( UnmanagedType.LPArray )] byte[] lpMultiByteStr,
          int cbMultiByte,
          [Out, MarshalAs( UnmanagedType.LPArray )] char[] lpWideCharStr,
          int cchWideChar );

      [DllImport( "kernel32.dll" )]
      internal static extern bool VirtualProtect( IntPtr lpAddress, UIntPtr dwSize, Protection flNewProtect, out Protection lpflOldProtect );
   }

   /// <summary>A bit-field of flags for protections</summary>
   [Flags]
   internal enum Protection
   {
      /// <summary>No access</summary>
      PAGE_NOACCESS = 0x01,
      /// <summary>Read only</summary>
      PAGE_READONLY = 0x02,
      /// <summary>Read write</summary>
      PAGE_READWRITE = 0x04,
      /// <summary>Write copy</summary>
      PAGE_WRITECOPY = 0x08,
      /// <summary>No access</summary>
      PAGE_EXECUTE = 0x10,
      /// <summary>Execute read</summary>
      PAGE_EXECUTE_READ = 0x20,
      /// <summary>Execute read write</summary>
      PAGE_EXECUTE_READWRITE = 0x40,
      /// <summary>Execute write copy</summary>
      PAGE_EXECUTE_WRITECOPY = 0x80,
      /// <summary>guard</summary>
      PAGE_GUARD = 0x100,
      /// <summary>No cache</summary>
      PAGE_NOCACHE = 0x200,
      /// <summary>Write combine</summary>
      PAGE_WRITECOMBINE = 0x400
   }
}
