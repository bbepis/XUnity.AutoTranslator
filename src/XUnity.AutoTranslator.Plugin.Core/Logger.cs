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
         if( Settings.EnableDebugLogs )
         {
            Log( LogLevel.Debug, message + Environment.NewLine + e );
         }
      }

      public void Debug( string message )
      {
         if( Settings.EnableDebugLogs )
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
               return "[XUnity.AutoTranslator][DEBUG]: ";
            case LogLevel.Info:
               return "[XUnity.AutoTranslator][INFO]: ";
            case LogLevel.Warn:
               return "[XUnity.AutoTranslator][WARN]: ";
            case LogLevel.Error:
               return "[XUnity.AutoTranslator][ERROR]: ";
            default:
               return "[XUnity.AutoTranslator][UNKNOW]: ";
         }
      }
   }
}
