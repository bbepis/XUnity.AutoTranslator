using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public abstract class XuaLogger
   {
      public static XuaLogger Current;

      public XuaLogger()
      {
         RespectSettings = true;
      }

      protected internal bool RespectSettings { get; protected set; }

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
               return "[DEBUG][XUnity.AutoTranslator " + PluginData.Version + "]: ";
            case LogLevel.Info:
               return "[INFO][XUnity.AutoTranslator " + PluginData.Version + "]: ";
            case LogLevel.Warn:
               return "[WARN][XUnity.AutoTranslator " + PluginData.Version + "]: ";
            case LogLevel.Error:
               return "[ERROR][XUnity.AutoTranslator " + PluginData.Version + "]: ";
            default:
               return "[UNKNOW][XUnity.AutoTranslator " + PluginData.Version + "]: ";
         }
      }
   }
}
