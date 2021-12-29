using System;
using System.Collections;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal class DebounceFunction
   {
      private readonly float _delaySeconds;
      private readonly Action _callback;
      private Coroutine _current;

      public DebounceFunction( float delaySeconds, Action callback )
      {
         _delaySeconds = delaySeconds;
         _callback = callback;
      }

      public void Execute()
      {
         if( _current != null )
         {
            CoroutineHelper.Stop( _current );
         }

         _current = CoroutineHelper.Start( Run() );
      }

      private IEnumerator Run()
      {
         yield return CoroutineHelper.CreateWaitForSeconds( _delaySeconds );

         _callback();

         _current = null;
      }
   }
}
