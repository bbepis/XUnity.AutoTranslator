using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using XUnity.Common.Logging;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class MaintenanceHelper
   {
      private static readonly object Sync = new object();
      private static readonly List<ActionRegistration> RegisteredActions = new List<ActionRegistration>();
      private static bool _initialized;

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="action"></param>
      /// <param name="filter"></param>
      public static void AddMaintenanceFunction( Action action, int filter )
      {
         lock( Sync )
         {
            if( !_initialized )
            {
               _initialized = true;
               StartMaintenance();
            }

            var registration = new ActionRegistration( action, filter );
            RegisteredActions.Add( registration );
         }
      }

      private static void StartMaintenance()
      {
         // start a thread that will periodically removed unused references
         var t1 = new Thread( MaintenanceLoop );
         t1.IsBackground = true;
         t1.Start();
      }

      private static void MaintenanceLoop( object state )
      {
         int i = 0;

         while( true )
         {
            lock( Sync )
            {
               foreach( var registration in RegisteredActions )
               {
                  if( i % registration.Filter == 0 )
                  {
                     try
                     {
                        registration.Action();
                     }
                     catch( Exception e )
                     {
                        XuaLogger.Common.Error( e, "An unexpected error occurred during maintenance." );
                     }
                  }
               }
            }

            i++;

            Thread.Sleep( 1000 * 5 );
         }
      }

      private class ActionRegistration
      {
         public ActionRegistration( Action action, int filter )
         {
            Action = action;
            Filter = filter;
         }

         public Action Action { get; }
         public int Filter { get; }
      }
   }
}
