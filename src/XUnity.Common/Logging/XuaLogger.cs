using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.Common.Logging
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public abstract class XuaLogger
   {
      private static XuaLogger _default;
      private static XuaLogger _common;
      private static XuaLogger _resourceRedirector;

      /// <summary>
      /// Gets the auto translator logger.
      /// </summary>
      public static XuaLogger AutoTranslator
      {
         get
         {
            if( _default == null )
            {
               _default = CreateLogger( "XUnity.AutoTranslator" );
            }
            return _default;
         }
         set
         {
            _default = value ?? throw new ArgumentNullException( "value" );
         }
      }

      /// <summary>
      /// Gets the common logger.
      /// </summary>
      public static XuaLogger Common
      {
         get
         {
            if( _common == null )
            {
               _common = CreateLogger( "XUnity.Common" );
            }
            return _common;
         }
         set
         {
            _common = value ?? throw new ArgumentNullException( "value" );
         }
      }

      /// <summary>
      /// Gets the resource redirector logger.
      /// </summary>
      public static XuaLogger ResourceRedirector
      {
         get
         {
            if( _resourceRedirector == null )
            {
               _resourceRedirector = CreateLogger( "XUnity.ResourceRedirector" );
            }
            return _resourceRedirector;
         }
         set
         {
            _resourceRedirector = value ?? throw new ArgumentNullException( "value" );
         }
      }

      internal static XuaLogger CreateLogger( string source )
      {
         try
         {
            return new ModLoaderSpecificLogger( source );
         }
         catch( Exception )
         {
            return new ConsoleLogger( source );
         }
      }

      /// <summary>
      /// Default constructor.
      /// </summary>
      public XuaLogger( string source )
      {
         Source = source;
      }

      /// <summary>
      /// Gets the source to be written to the log.
      /// </summary>
      public string Source { get; set; }

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
               return "[DEBUG][" + Source + "]: ";
            case LogLevel.Info:
               return "[INFO][" + Source + "]: ";
            case LogLevel.Warn:
               return "[WARN][" + Source + "]: ";
            case LogLevel.Error:
               return "[ERROR][" + Source + "]: ";
            default:
               return "[UNKNOW][" + Source + "]: ";
         }
      }
   }
}
