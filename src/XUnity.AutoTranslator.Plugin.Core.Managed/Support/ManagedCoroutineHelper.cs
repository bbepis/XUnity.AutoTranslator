using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal class ManagedCoroutineHelper : ICoroutineHelper
   {
      public object CreateWaitForSeconds( float seconds )
      {
         return new WaitForSeconds( seconds );
      }

      public object CreateWaitForSecondsRealtime( float delay )
      {
         if( UnityFeatures.SupportsWaitForSecondsRealtime )
         {
            return GetWaitForSecondsRealtimeInternal( delay );
         }
         return null;
      }

      private static IEnumerator GetWaitForSecondsRealtimeInternal( float delay )
      {
         return new WaitForSecondsRealtime( delay );
      }

      public object Start( IEnumerator coroutine )
      {
         return AutoTranslationPlugin.Current.StartCoroutine( coroutine );
      }

      public void Stop( object coroutine )
      {
         AutoTranslationPlugin.Current.StopCoroutine( (Coroutine)coroutine );
      }

      public bool SupportsCustomYieldInstruction()
      {
         return UnityFeatures.SupportsCustomYieldInstruction;
      }
   }
}
