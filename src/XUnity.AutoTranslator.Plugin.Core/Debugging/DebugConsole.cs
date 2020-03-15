using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Debugging
{
   internal static class DebugConsole
   {
      private static IntPtr _consoleOut;

      public static void Enable()
      {
         if( Settings.EnableConsole )
         {
            TryEnableConsole();
         }
      }

      private static bool TryEnableConsole()
      {
         try
         {
            var oldConsoleOut = Kernel32.GetStdHandle( -11 );
            if( !Kernel32.AllocConsole() ) return false;

            _consoleOut = Kernel32.CreateFile( "CONOUT$", 0x40000000, 2, IntPtr.Zero, 3, 0, IntPtr.Zero );
            if( !Kernel32.SetStdHandle( -11, _consoleOut ) ) return false;

            Stream stream = Console.OpenStandardOutput();
            StreamWriter writer = new StreamWriter( stream, Encoding.Default );
            writer.AutoFlush = true;

            Console.SetOut( writer );
            Console.SetError( writer );

            uint shiftjisCodePage = 932;

            Kernel32.SetConsoleOutputCP( shiftjisCodePage );
            Console.OutputEncoding = ConsoleEncoding.GetEncoding( shiftjisCodePage );

            return true;
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred during while enabling console." );
         }

         return false;
      }
   }
}
