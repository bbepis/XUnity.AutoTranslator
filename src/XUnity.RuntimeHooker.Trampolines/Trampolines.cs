using System;
using System.Text;
using System.Threading;
using XUnity.RuntimeHooker.Core;

namespace XUnity.RuntimeHooker.Trampolines
{
   #region InstanceTrampolineWithReturn

   internal static class InstanceTrampolineWithReturn<T1>
   {
      static object Override( T1 t1 )
      {
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            object result = TrampolineHandler.Func( t1, TrampolineInitializer.Data );

            return result;
         }
         finally
         {
            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class InstanceTrampolineWithReturn<T1, T2>
   {
      static object Override( T1 t1, T2 t2 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t2;
            object result = TrampolineHandler.Func( t1, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class InstanceTrampolineWithReturn<T1, T2, T3>
   {
      static object Override( T1 t1, T2 t2, T3 t3 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t2;
            data.Parameters[ 1 ] = t3;
            object result = TrampolineHandler.Func( t1, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class InstanceTrampolineWithReturn<T1, T2, T3, T4>
   {
      static object Override( T1 t1, T2 t2, T3 t3, T4 t4 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t2;
            data.Parameters[ 1 ] = t3;
            data.Parameters[ 2 ] = t4;
            object result = TrampolineHandler.Func( t1, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class InstanceTrampolineWithReturn<T1, T2, T3, T4, T5>
   {
      static object Override( T1 t1, T2 t2, T3 t3, T4 t4, T5 t5 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t2;
            data.Parameters[ 1 ] = t3;
            data.Parameters[ 2 ] = t4;
            data.Parameters[ 3 ] = t5;
            object result = TrampolineHandler.Func( t1, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;
            data.Parameters[ 3 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   #endregion

   #region InstanceTrampoline

   internal static class InstanceTrampoline<T1>
   {
      static void Override( T1 t1 )
      {
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            TrampolineHandler.Action( t1, TrampolineInitializer.Data );
         }
         finally
         {
            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class InstanceTrampoline<T1, T2>
   {
      static void Override( T1 t1, T2 t2 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t2;
            TrampolineHandler.Action( t1, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class InstanceTrampoline<T1, T2, T3>
   {
      static void Override( T1 t1, T2 t2, T3 t3 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t2;
            data.Parameters[ 1 ] = t3;
            TrampolineHandler.Action( t1, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class InstanceTrampoline<T1, T2, T3, T4>
   {
      static void Override( T1 t1, T2 t2, T3 t3, T4 t4 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t2;
            data.Parameters[ 1 ] = t3;
            data.Parameters[ 2 ] = t4;
            TrampolineHandler.Action( t1, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class InstanceTrampoline<T1, T2, T3, T4, T5>
   {
      static void Override( T1 t1, T2 t2, T3 t3, T4 t4, T5 t5 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t2;
            data.Parameters[ 1 ] = t3;
            data.Parameters[ 2 ] = t4;
            data.Parameters[ 3 ] = t5;
            TrampolineHandler.Action( t1, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;
            data.Parameters[ 3 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   #endregion

   #region StaticTrampolineWithReturn

   internal static class StaticTrampolineWithReturn
   {
      static object Override()
      {
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            object result = TrampolineHandler.Func( null, TrampolineInitializer.Data );

            return result;
         }
         finally
         {
            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampolineWithReturn<T1>
   {
      static object Override( T1 t1 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            object result = TrampolineHandler.Func( null, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampolineWithReturn<T1, T2>
   {
      static object Override( T1 t1, T2 t2 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            data.Parameters[ 1 ] = t2;
            object result = TrampolineHandler.Func( null, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampolineWithReturn<T1, T2, T3>
   {
      static object Override( T1 t1, T2 t2, T3 t3 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            data.Parameters[ 1 ] = t2;
            data.Parameters[ 2 ] = t3;
            object result = TrampolineHandler.Func( null, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampolineWithReturn<T1, T2, T3, T4>
   {
      static object Override( T1 t1, T2 t2, T3 t3, T4 t4 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            data.Parameters[ 1 ] = t2;
            data.Parameters[ 2 ] = t3;
            data.Parameters[ 3 ] = t4;
            object result = TrampolineHandler.Func( null, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;
            data.Parameters[ 3 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampolineWithReturn<T1, T2, T3, T4, T5>
   {
      static object Override( T1 t1, T2 t2, T3 t3, T4 t4, T5 t5 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            data.Parameters[ 1 ] = t2;
            data.Parameters[ 2 ] = t3;
            data.Parameters[ 3 ] = t4;
            data.Parameters[ 4 ] = t5;
            object result = TrampolineHandler.Func( null, data );

            return result;
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;
            data.Parameters[ 3 ] = null;
            data.Parameters[ 4 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   #endregion

   #region StaticTrampoline

   internal static class StaticTrampoline
   {
      static void Override()
      {
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            TrampolineHandler.Action( null, TrampolineInitializer.Data );
         }
         finally
         {
            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampoline<T1>
   {
      static void Override( T1 t1 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            TrampolineHandler.Action( null, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampoline<T1, T2>
   {
      static void Override( T1 t1, T2 t2 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            data.Parameters[ 1 ] = t2;
            TrampolineHandler.Action( null, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampoline<T1, T2, T3>
   {
      static void Override( T1 t1, T2 t2, T3 t3 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            data.Parameters[ 1 ] = t2;
            data.Parameters[ 2 ] = t3;
            TrampolineHandler.Action( null, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampoline<T1, T2, T3, T4>
   {
      static void Override( T1 t1, T2 t2, T3 t3, T4 t4 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            data.Parameters[ 1 ] = t2;
            data.Parameters[ 2 ] = t3;
            data.Parameters[ 3 ] = t4;
            TrampolineHandler.Action( null, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;
            data.Parameters[ 3 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   internal static class StaticTrampoline<T1, T2, T3, T4, T5>
   {
      static void Override( T1 t1, T2 t2, T3 t3, T4 t4, T5 t5 )
      {
         var data = TrampolineInitializer.Data;
         try
         {
            Monitor.Enter( TrampolineInitializer.Sync );

            data.Parameters[ 0 ] = t1;
            data.Parameters[ 1 ] = t2;
            data.Parameters[ 2 ] = t3;
            data.Parameters[ 3 ] = t4;
            data.Parameters[ 4 ] = t5;
            TrampolineHandler.Action( null, data );
         }
         finally
         {
            data.Parameters[ 0 ] = null;
            data.Parameters[ 1 ] = null;
            data.Parameters[ 2 ] = null;
            data.Parameters[ 3 ] = null;
            data.Parameters[ 4 ] = null;

            Monitor.Exit( TrampolineInitializer.Sync );
         }
      }
   }

   #endregion
}
