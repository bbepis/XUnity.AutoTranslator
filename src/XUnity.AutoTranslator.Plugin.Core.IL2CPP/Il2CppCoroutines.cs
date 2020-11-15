using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.IL2CPP
{
   internal static class Il2CppCoroutines
   {
      private struct CoroutineAndCurrentWaitCondition
      {
         public object WaitCondition;
         public IEnumerator Coroutine;
      }

      private class Il2CppEnumeratorWrapper : IEnumerator
      {
         private readonly Il2CppSystem.Collections.IEnumerator il2cppEnumerator;

         public Il2CppEnumeratorWrapper( Il2CppSystem.Collections.IEnumerator il2CppEnumerator ) => il2cppEnumerator = il2CppEnumerator;
         public bool MoveNext() => il2cppEnumerator.MoveNext();
         public void Reset() => il2cppEnumerator.Reset();
         public object Current => il2cppEnumerator.Current;
      }

      private static readonly List<CoroutineAndCurrentWaitCondition> _coroutines = new List<CoroutineAndCurrentWaitCondition>();
      private static readonly List<IEnumerator> _coroutinesForNextFrame = new List<IEnumerator>();
      private static readonly List<IEnumerator> _temp = new List<IEnumerator>();

      internal static object Start( IEnumerator routine )
      {
         if( routine != null )
         {
            ProcessNextOfCoroutine( routine );
         }

         return routine;
      }

      internal static void Stop( IEnumerator enumerator )
      {
         if( _coroutinesForNextFrame.Contains( enumerator ) ) // the coroutine is running itself
         {
            _coroutinesForNextFrame.Remove( enumerator );
         }
         else
         {
            int coroTupleIndex = _coroutines.FindIndex( c => c.Coroutine == enumerator );
            if( coroTupleIndex != -1 ) // the coroutine is waiting for a subroutine
            {
               object waitCondition = _coroutines[ coroTupleIndex ].WaitCondition;
               if( waitCondition is IEnumerator waitEnumerator )
               {
                  Stop( waitEnumerator );
               }

               _coroutines.RemoveAt( coroTupleIndex );
            }
         }
      }

      internal static void ProcessPostUpdate()
      {
         for( var i = _coroutines.Count - 1; i >= 0; i-- )
         {
            var tuple = _coroutines[ i ];
            if( tuple.WaitCondition is WaitForSeconds waitForSeconds )
            {
               if( ( waitForSeconds.m_Seconds -= Time.deltaTime ) <= 0 )
               {
                  _coroutines.RemoveAt( i );
                  ProcessNextOfCoroutine( tuple.Coroutine );
               }
            }
         }

         ProcessCoroutinesForNextFrame();
      }

      private static void ProcessCoroutinesForNextFrame()
      {
         if( _coroutinesForNextFrame.Count == 0 ) return;

         // use a temp list to make sure waits made during processing are not handled by same processing invocation
         // additionally, a temp list reduces allocations compared to an array
         _temp.AddRange( _coroutinesForNextFrame );
         _coroutinesForNextFrame.Clear();
         foreach( var enumerator in _temp )
         {
            ProcessNextOfCoroutine( enumerator );
         }
         _temp.Clear();
      }

      private static void ProcessNextOfCoroutine( IEnumerator enumerator )
      {
         try
         {
            if( !enumerator.MoveNext() ) // Run the next step of the coroutine. If it's done, restore the parent routine
            {
               var indices = _coroutines
                  .Select( ( it, idx ) => (idx, it) )
                  .Where( it => it.it.WaitCondition == enumerator )
                  .Select( it => it.idx )
                  .ToList();

               for( var i = indices.Count - 1; i >= 0; i-- )
               {
                  var index = indices[ i ];
                  _coroutinesForNextFrame.Add( _coroutines[ index ].Coroutine );
                  _coroutines.RemoveAt( index );
               }
               return;
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while processing coroutines." );
            Stop( FindOriginalCoro( enumerator ) ); // We want the entire coroutine hierachy to stop when an error happen
         }

         var next = enumerator.Current;
         switch( next )
         {
            case null:
               _coroutinesForNextFrame.Add( enumerator );
               return;
            case WaitForSeconds _:
               break; // do nothing, this one is supported in Process
            case Il2CppObjectBase il2CppObjectBase:
               var nextAsEnumerator = il2CppObjectBase.TryCast<Il2CppSystem.Collections.IEnumerator>();
               if( nextAsEnumerator != null ) // il2cpp IEnumerator also handles CustomYieldInstruction
               {
                  next = new Il2CppEnumeratorWrapper( nextAsEnumerator );
               }
               else
               {
                  XuaLogger.AutoTranslator.Warn( "Unsupported coroutine object type: " + next.GetType().Name );
               }
               break;
         }

         _coroutines.Add( new CoroutineAndCurrentWaitCondition { WaitCondition = next, Coroutine = enumerator } );

         if( next is IEnumerator nextCoro )
         {
            ProcessNextOfCoroutine( nextCoro );
         }
      }

      private static IEnumerator FindOriginalCoro( IEnumerator enumerator )
      {
         int index = _coroutines.FindIndex( ct => ct.WaitCondition == enumerator );
         if( index == -1 )
         {
            return enumerator;
         }
         return FindOriginalCoro( _coroutines[ index ].Coroutine );
      }
   }
}
