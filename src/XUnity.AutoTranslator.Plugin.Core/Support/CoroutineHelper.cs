using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class CoroutineHelper
   {
      public static WaitForSeconds CreateWaitForSeconds( float seconds )
      {
         return new WaitForSeconds( seconds );
      }

      public static IEnumerator CreateWaitForSecondsRealtime( float delay )
      {
         // Could be bad... WaitForSecondsRealtime could be shimmed away, even if IEnumerator is supported
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

      public static bool SupportsCustomYieldInstruction()
      {
         return UnityFeatures.SupportsCustomYieldInstruction;
      }

#if IL2CPP
      public static object Start( IEnumerator coroutine )
      {
         return Il2CppCoroutines.Start( coroutine );
      }

      public static void Stop( object coroutine )
      {
         Il2CppCoroutines.Stop( (IEnumerator)coroutine );
      }
#endif

#if MANAGED
      public static object Start( IEnumerator coroutine )
      {
         return AutoTranslationPlugin.Current.StartCoroutine( coroutine );
      }

      public static void Stop( object coroutine )
      {
         AutoTranslationPlugin.Current.StopCoroutine( (Coroutine)coroutine );
      }
#endif
   }
}
