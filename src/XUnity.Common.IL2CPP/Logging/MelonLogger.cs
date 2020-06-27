using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnity.Common.Logging;

namespace XUnity.Common.IL2CPP.Logging
{
   public class MelonLogger : XuaLogger
   {
      public MelonLogger( string source ) : base( source )
      {
      }

      protected override void Log( LogLevel level, string message )
      {
         switch( level )
         {
            case LogLevel.Debug:
               MelonModLogger.Log( ConsoleColor.Gray, message );
               break;
            case LogLevel.Info:
               MelonModLogger.Log( message );
               break;
            case LogLevel.Warn:
               MelonModLogger.LogWarning( message );
               break;
            case LogLevel.Error:
               MelonModLogger.LogError( message );
               break;
            default:
               break;
         }
      }
   }
}
