using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace XUnity.RuntimeHooker.Unmanaged
{
   internal static class Kernel32
   {
      [DllImport( "kernel32.dll" )]
      internal static extern bool VirtualProtect( IntPtr lpAddress, UIntPtr dwSize, Protection flNewProtect, out Protection lpflOldProtect );
   }
}
