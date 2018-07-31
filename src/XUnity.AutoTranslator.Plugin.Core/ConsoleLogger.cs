using System;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public class ConsoleLogger : Logger
   {
      protected override void Write( string formattedMessage )
      {
         Console.WriteLine( formattedMessage );
      }
   }
}
