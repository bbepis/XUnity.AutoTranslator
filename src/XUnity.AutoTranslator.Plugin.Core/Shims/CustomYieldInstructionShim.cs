using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   /// <summary>
   /// Shim for CustomYieldInstruction because it is not supported before version 5.3 of Unity.
   /// </summary>
   public abstract class CustomYieldInstructionShim : IEnumerator
   {
      private float? _startTime;

      /// <summary>
      /// Default constructor.
      /// </summary>
      protected CustomYieldInstructionShim()
      {
      }

      internal bool DetermineKeepWaiting()
      {
         if( !keepWaiting || IsTimedOut ) return false;
         
         if( InGameTimeout.HasValue )
         {
            if( !_startTime.HasValue )
            {
               _startTime = TimeSupport.Time.realtimeSinceStartup;
            }

            var startTime = _startTime.Value;
            var time = TimeSupport.Time.realtimeSinceStartup - startTime;

            if( time > InGameTimeout )
            {
               IsTimedOut = true;

               return false;
            }
         }

         return true;
      }

      /// <summary>
      /// Checks if the yield instruction needs to keep waiting.
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
         return DetermineKeepWaiting();
      }

      /// <summary>
      /// Does nothing.
      /// </summary>
      public void Reset()
      {
      }

      /// <summary>
      /// Gets a null object, specifying to Unity that it must
      /// wait a single frame before calling MoveNext again.
      /// </summary>
      public object Current
      {
         get
         {
            return null;
         }
      }

      /// <summary>
      /// Gets a bool indicating if the yield instruction
      /// needs to keep waiting.
      /// </summary>
      public abstract bool keepWaiting { get; }

      /// <summary>
      /// Gets or sets a timeout in in-game seconds.
      /// </summary>
      public float? InGameTimeout { get; set; }

      /// <summary>
      /// Gets or sets a bool indicating if the CustomYieldOperation timed out.
      /// </summary>
      public bool IsTimedOut { get; set; }

      /// <summary>
      /// Gets an enumerator that can be iterated through
      /// in a co-routine and will work in even ancient versions
      /// of Unity.
      /// </summary>
      /// <returns></returns>
      public IEnumerator GetSupportedEnumerator()
      {
         if( UnityFeatures.SupportsCustomYieldInstruction ) // requires Unity 5.3 or later
         {
            yield return this;
         }
         else
         {
            while( DetermineKeepWaiting() )
            {
               yield return CoroutineHelper.CreateWaitForSeconds( 0.2f );
            }
         }
      }
   }
}
