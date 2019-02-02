using System;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class ConsoleLogger : Logger
   {
      protected override void Log( LogLevel level, string message )
      {
         Console.WriteLine( $"{GetPrefix( level )} {message}" );
      }
   }
}
