using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public abstract class Logger
   {
      public static Logger Current;

      public Logger()
      {
         RespectSettings = true;
      }

      public bool RespectSettings { get; protected set; }

      public void Error( Exception e, string message )
      {
         Log( LogLevel.Error, message + Environment.NewLine + e );
      }

      public void Error( string message )
      {
         Log( LogLevel.Error, message );
      }

      public void Warn( Exception e, string message )
      {
         Log( LogLevel.Warn, message + Environment.NewLine + e );
      }

      public void Warn( string message )
      {
         Log( LogLevel.Warn, message );
      }

      public void Info( Exception e, string message )
      {
         Log( LogLevel.Info, message + Environment.NewLine + e );
      }

      public void Info( string message )
      {
         Log( LogLevel.Info, message );
      }

      public void Debug( Exception e, string message )
      {
         if( Settings.EnableDebugLogs || !RespectSettings )
         {
            Log( LogLevel.Debug, message + Environment.NewLine + e );
         }
      }

      public void Debug( string message )
      {
         if( Settings.EnableDebugLogs || !RespectSettings )
         {
            Log( LogLevel.Debug, message );
         }
      }

      protected abstract void Log( LogLevel level, string message );

      protected string GetPrefix( LogLevel level )
      {
         switch( level )
         {
            case LogLevel.Debug:
               return "[DEBUG][XUnity.AutoTranslator]: ";
            case LogLevel.Info:
               return "[INFO][XUnity.AutoTranslator]: ";
            case LogLevel.Warn:
               return "[WARN][XUnity.AutoTranslator]: ";
            case LogLevel.Error:
               return "[ERROR][XUnity.AutoTranslator]: ";
            default:
               return "[UNKNOW][XUnity.AutoTranslator]: ";
         }
      }
   }
}
