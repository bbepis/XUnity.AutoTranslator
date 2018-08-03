using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core;

namespace XUnity.AutoTranslator.Plugin.BepIn
{
   public class BepInLogger : Logger
   {
      protected override void Log( LogLevel level, string message )
      {
         BepInEx.BepInLogger.Log( message );
      }
   }
}
