using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal static class ApplicationInformation
   {
      [DllImport( "kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false )]
      private static extern int GetModuleFileName( HandleRef hModule, StringBuilder buffer, int length );

      private static HandleRef Null = new HandleRef( null, IntPtr.Zero );

      public static string StartupPath
      {
         get
         {
            StringBuilder stringBuilder = new StringBuilder( 260 );
            GetModuleFileName( Null, stringBuilder, stringBuilder.Capacity );
            return stringBuilder.ToString();
         }
      }
   }
}
