using System;
using System.Collections;
using XUnity.AutoTranslator.Plugin.Core.Support;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal class DebounceFunction
   {
      private readonly float _delaySeconds;
      private readonly Action _callback;
      private object _current;

      public DebounceFunction( float delaySeconds, Action callback )
      {
         _delaySeconds = delaySeconds;
         _callback = callback;
      }

      public void Execute()
      {
         if( _current != null )
         {
            CoroutineHelper.Instance.Stop( _current );
         }

         _current = CoroutineHelper.Instance.Start( Run() );
      }

      private IEnumerator Run()
      {
         yield return CoroutineHelper.Instance.CreateWaitForSeconds( _delaySeconds );

         _callback();

         _current = null;
      }
   }
}
