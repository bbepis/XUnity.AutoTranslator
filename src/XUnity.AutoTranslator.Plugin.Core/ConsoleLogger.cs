using System;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class ConsoleLogger : XuaLogger
   {
      protected override void Log( LogLevel level, string message )
      {
         Console.WriteLine( $"{GetDefaultPrefix( level )} {message}" );
      }
   }
}
