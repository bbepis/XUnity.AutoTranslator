using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.Common.Logging
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
      private static XuaLogger _instance;

      /// <summary>
      /// Gets the current XuaLogger.
      /// </summary>
      public static XuaLogger Current
      {
         get
         {
            if( _instance == null )
            {
               try
               {
                  _instance = new BepInExLogger();
               }
               catch( Exception )
               {
                  _instance = new ConsoleLogger();
               }
            }
            return _instance;
         }
         set
         {
            _instance = value ?? throw new ArgumentNullException( "value" );
         }
      }

      /// <summary>
      /// Default constructor.
      /// </summary>
      public XuaLogger()
      {
      }

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
         Log( LogLevel.Debug, message + Environment.NewLine + e );
      }

      /// <summary>
      /// Logs a message at debug level.
      /// </summary>
      /// <param name="message">The message to log.</param>
      public void Debug( string message )
      {
         Log( LogLevel.Debug, message );
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
               return "[DEBUG][XUnity]: ";
            case LogLevel.Info:
               return "[INFO][XUnity]: ";
            case LogLevel.Warn:
               return "[WARN][XUnity]: ";
            case LogLevel.Error:
               return "[ERROR][XUnity]: ";
            default:
               return "[UNKNOW][XUnity]: ";
         }
      }
   }
}
