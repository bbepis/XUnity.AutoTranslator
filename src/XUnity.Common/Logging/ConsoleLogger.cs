using System;

namespace XUnity.Common.Logging
{
   internal class ConsoleLogger : XuaLogger
   {
      protected override void Log( LogLevel level, string message )
      {
         Console.WriteLine( $"{GetDefaultPrefix( level )} {message}" );
      }
   }
}
