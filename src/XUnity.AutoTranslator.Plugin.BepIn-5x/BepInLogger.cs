using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.BepIn_5x
{
   public class BepInLogger : XuaLogger
   {
      private readonly ManualLogSource _logger = Logger.CreateLogSource( "XUnity.AutoTranslator " + PluginData.Version );

      public BepInLogger()
      {
         RespectSettings = false;
      }

      protected override void Log( Core.LogLevel level, string message )
      {
         _logger.Log( Convert( level ), message );
      }

      public BepInEx.Logging.LogLevel Convert( Core.LogLevel level )
      {
         switch( level )
         {
            case Core.LogLevel.Debug:
               return BepInEx.Logging.LogLevel.Debug;
            case Core.LogLevel.Info:
               return BepInEx.Logging.LogLevel.Info;
            case Core.LogLevel.Warn:
               return BepInEx.Logging.LogLevel.Warning;
            case Core.LogLevel.Error:
               return BepInEx.Logging.LogLevel.Error;
            default:
               return BepInEx.Logging.LogLevel.None;
         }
      }
   }
}
