using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.Common.Utilities;

namespace XUnity.Common.Logging
{
   internal class ModLoaderSpecificLogger : XuaLogger
   {
      private static Action<LogLevel, string> _logMethod;

      public ModLoaderSpecificLogger( string source )
         : base( source )
      {
         if( _logMethod != null )
            return;

         var staticFlags = BindingFlags.Static | BindingFlags.Public;
         var instanceFlags = BindingFlags.Instance | BindingFlags.Public;

         var logLevelType = Type.GetType( "BepInEx.Logging.LogLevel, BepInEx", false ) ?? Type.GetType( "BepInEx.Logging.LogLevel, BepInEx.Core", false );
         if( logLevelType != null )
         {
            var manualLogSourceType = Type.GetType( "BepInEx.Logging.ManualLogSource, BepInEx", false ) ?? Type.GetType( "BepInEx.Logging.ManualLogSource, BepInEx.Core", false );
            if( manualLogSourceType != null )
            {
               var loggerType = Type.GetType( "BepInEx.Logging.Logger, BepInEx", false ) ?? Type.GetType( "BepInEx.Logging.Logger, BepInEx.Core", false );
               var createLogSourceMethod = loggerType.GetMethod( "CreateLogSource", staticFlags, null, new[] { typeof( string ) }, null );
               var logInstance = createLogSourceMethod.Invoke( null, new object[] { Source } );
               var logMethod = logInstance.GetType().GetMethod( "Log", instanceFlags, null, new[] { logLevelType, typeof( object ) }, null );
               var log = CustomFastReflectionHelper.CreateFastDelegate( logMethod );

               _logMethod = (level, msg) => 
               {
                  var bepinexLevel = Convert( level );
                  log( logInstance, new object[] { bepinexLevel, msg } );
               };
            }
            else
            {
               var loggerType = Type.GetType( "BepInEx.Logger, BepInEx", false );
               var logInstance = loggerType.GetProperty( "CurrentLogger", staticFlags ).GetValue( null, null );
               var logMethod = logInstance.GetType().GetMethod( "Log", instanceFlags, null, new[] { logLevelType, typeof( object ) }, null );
               var log = CustomFastReflectionHelper.CreateFastDelegate( logMethod );

               _logMethod = ( level, msg ) =>
               {
                  var bepinexLevel = Convert( level );
                  log( logInstance, new object[] { bepinexLevel, msg } );
               };
            }
         }
         else
         {
            var melonModLogger = Type.GetType( "MelonLoader.MelonLogger, MelonLoader.ModHandler", false );
            if( melonModLogger != null )
            {
               var logDebugMethod = melonModLogger.GetMethod( "Log", staticFlags, null, new Type[] { typeof( ConsoleColor ), typeof( string ) }, null );
               var logInfoMethod = melonModLogger.GetMethod( "Log", staticFlags, null, new Type[] { typeof( string ) }, null );
               var logWarningMethod = melonModLogger.GetMethod( "LogWarning", staticFlags, null, new Type[] { typeof( string ) }, null );
               var logErrorMethod = melonModLogger.GetMethod( "LogError", staticFlags, null, new Type[] { typeof( string ) }, null );

               var logDebug = CustomFastReflectionHelper.CreateFastDelegate( logDebugMethod );
               var logInfo = CustomFastReflectionHelper.CreateFastDelegate( logInfoMethod );
               var logWarning = CustomFastReflectionHelper.CreateFastDelegate( logWarningMethod );
               var logError = CustomFastReflectionHelper.CreateFastDelegate( logErrorMethod );

               _logMethod = ( level, msg ) =>
               {
                  switch( level )
                  {
                     case LogLevel.Debug:
                        logDebug( null, ConsoleColor.Gray, msg );
                        break;
                     case LogLevel.Info:
                        logInfo( null, msg );
                        break;
                     case LogLevel.Warn:
                        logWarning( null, msg );
                        break;
                     case LogLevel.Error:
                        logError( null, msg );
                        break;
                     default:
                        throw new ArgumentException( "level" );
                  }
               };
            }
         }

         if( _logMethod == null )
         {
            throw new Exception( "Did not recognize any mod loader!" );
         }
      }

      protected override void Log( LogLevel level, string message )
      {
         _logMethod( level, message );
      }

      public static int Convert( LogLevel level )
      {
         switch( level )
         {
            case LogLevel.Debug:
               return BepInExLogLevel.Debug;
            case LogLevel.Info:
               return BepInExLogLevel.Info;
            case LogLevel.Warn:
               return BepInExLogLevel.Warning;
            case LogLevel.Error:
               return BepInExLogLevel.Error;
            default:
               return BepInExLogLevel.None;
         }
      }

      public static class BepInExLogLevel
      {
         public const int None = 0;
         public const int Fatal = 1;
         public const int Error = 2;
         public const int Warning = 4;
         public const int Message = 8;
         public const int Info = 16;
         public const int Debug = 32;
         public const int All = 63;
      }
   }
}
