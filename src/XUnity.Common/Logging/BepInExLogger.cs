using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.Common.Utilities;

namespace XUnity.Common.Logging
{
   internal class BepInExLogger : XuaLogger
   {
      private readonly object _logObject;
      private Func<object, object[], object> _logMethod;

      public BepInExLogger()
      {
         var staticFlags = BindingFlags.Static | BindingFlags.Public;
         var instanceFlags = BindingFlags.Instance | BindingFlags.Public;

         var logLevelType = Type.GetType( "BepInEx.Logging.LogLevel, BepInEx", false );
         if( logLevelType == null ) throw new Exception( "BepInEx is not loaded!" );

         var manualLogSourceType = Type.GetType( "BepInEx.Logging.ManualLogSource, BepInEx", false );
         if( manualLogSourceType != null )
         {
            var loggerType = Type.GetType( "BepInEx.Logging.Logger, BepInEx", false );
            var createLogSourceMethod = loggerType.GetMethod( "CreateLogSource", staticFlags, null, new[] { typeof( string ) }, null );
            _logObject = createLogSourceMethod.Invoke( null, new object[] { "XUnity" } );
            var logMethod = _logObject.GetType().GetMethod( "Log", instanceFlags, null, new[] { logLevelType, typeof( object ) }, null );
            _logMethod = ExpressionHelper.CreateFastInvoke( logMethod );
         }
         else
         {
            var loggerType = Type.GetType( "BepInEx.Logger, BepInEx", false );
            _logObject = loggerType.GetProperty( "CurrentLogger", staticFlags ).GetValue( null, null );
            var logMethod = _logObject.GetType().GetMethod( "Log", instanceFlags, null, new[] { logLevelType, typeof( object ) }, null );
            _logMethod = ExpressionHelper.CreateFastInvoke( logMethod );
         }

         if( _logMethod == null )
         {
            throw new Exception( "BepInEx is not loaded!" );
         }
      }

      protected override void Log( LogLevel level, string message )
      {
         _logMethod( _logObject, new object[] { Convert( level ), message } );
      }

      public int Convert( LogLevel level )
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
