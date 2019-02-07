using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Use this class to send log messaages to the console with.
   ///
   /// You should not use a plugin-manager supplied logger. This
   /// logger will automatically delegate the logs to the correct
   /// logger based on which plugin framework it is used in.
   /// </summary>
   public abstract class XuaLogger
   {
      /// <summary>
      /// Gets the current XuaLogger.
      /// </summary>
      public static XuaLogger Current;

      /// <summary>
      /// Default constructor.
      /// </summary>
      public XuaLogger()
      {
         RespectSettings = true;
      }

      /// <summary>
      /// Gets a bool whether or not the settings supplied in the
      /// configuration file for auto translator should be honored.
      /// </summary>
      protected internal bool RespectSettings { get; protected set; }

      /// <summary>
      /// Logs a message and exception at error level.
      /// </summary>
      /// <param name="e">The exception to log.</param>
      /// <param name="message">The message to log.</param>
      public void Error( Exception e, string message )
      {
         Log( LogLevel.Error, message + Environment.NewLine + e );
      }

      /// <summary>
      /// Logs a message at error level.
      /// </summary>
      /// <param name="message">The message to log.</param>
      public void Error( string message )
      {
         Log( LogLevel.Error, message );
      }

      /// <summary>
      /// Logs a message and exception at warn level.
      /// </summary>
      /// <param name="e"></param>
      /// <param name="message"></param>
      public void Warn( Exception e, string message )
      {
         Log( LogLevel.Warn, message + Environment.NewLine + e );
      }

      /// <summary>
      /// Logs a message at warn level.
      /// </summary>
      /// <param name="message">The message to log.</param>
      public void Warn( string message )
      {
         Log( LogLevel.Warn, message );
      }

      /// <summary>
      /// Logs a message and exception at info level.
      /// </summary>
      /// <param name="e"></param>
      /// <param name="message"></param>
      public void Info( Exception e, string message )
      {
         Log( LogLevel.Info, message + Environment.NewLine + e );
      }

      /// <summary>
      /// Logs a message at info level.
      /// </summary>
      /// <param name="message">The message to log.</param>
      public void Info( string message )
      {
         Log( LogLevel.Info, message );
      }

      /// <summary>
      /// Logs a message and exception at debug level.
      /// </summary>
      /// <param name="e"></param>
      /// <param name="message"></param>
      public void Debug( Exception e, string message )
      {
         if( Settings.EnableDebugLogs || !RespectSettings )
         {
            Log( LogLevel.Debug, message + Environment.NewLine + e );
         }
      }

      /// <summary>
      /// Logs a message at debug level.
      /// </summary>
      /// <param name="message">The message to log.</param>
      public void Debug( string message )
      {
         if( Settings.EnableDebugLogs || !RespectSettings )
         {
            Log( LogLevel.Debug, message );
         }
      }

      /// <summary>
      /// Logs a message at the specified level.
      /// </summary>
      /// <param name="level"></param>
      /// <param name="message"></param>
      protected abstract void Log( LogLevel level, string message );

      /// <summary>
      /// Gets the default prefix for a log message at the specific level.
      /// </summary>
      /// <param name="level"></param>
      /// <returns></returns>
      protected string GetDefaultPrefix( LogLevel level )
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
