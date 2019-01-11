using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.BepIn
{
   public class BepInLogger : Logger
   {
      public BepInLogger()
      {
         RespectSettings = false;
      }

      protected override void Log( LogLevel level, string message )
      {
         BepInEx.Logger.CurrentLogger.Log( Convert( level ), "[XUnity.AutoTranslator " + PluginData.Version + "] " + message );
      }

      public BepInEx.Logging.LogLevel Convert( LogLevel level )
      {
         switch( level )
         {
            case LogLevel.Debug:
               return BepInEx.Logging.LogLevel.Debug;
            case LogLevel.Info:
               return BepInEx.Logging.LogLevel.Info;
            case LogLevel.Warn:
               return BepInEx.Logging.LogLevel.Warning;
            case LogLevel.Error:
               return BepInEx.Logging.LogLevel.Error;
            default:
               return BepInEx.Logging.LogLevel.None;
         }
      }
   }
}
