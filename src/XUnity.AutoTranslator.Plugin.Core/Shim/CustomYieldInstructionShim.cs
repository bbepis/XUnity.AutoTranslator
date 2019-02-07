using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Shim
{
   /// <summary>
   /// Shim for CustomYieldInstruction because it is not supported before version 5.3 of Unity.
   /// </summary>
   public abstract class CustomYieldInstructionShim : IEnumerator
   {
      /// <summary>
      /// Default constructor.
      /// </summary>
      protected CustomYieldInstructionShim()
      {
      }

      /// <summary>
      /// Checks if the yield instruction needs to keep waiting.
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
         return keepWaiting;
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
      /// Gets an enumerator that can be iterated through
      /// in a co-routine and will work in even ancient versions
      /// of Unity.
      /// </summary>
      /// <returns></returns>
      public IEnumerator GetSupportedEnumerator()
      {
         if( Features.SupportsCustomYieldInstruction ) // requires Unity 5.3 or later
         {
            yield return this;
         }
         else
         {
            while( keepWaiting )
            {
               yield return new WaitForSeconds( 0.2f );
            }
         }
      }
   }
}
