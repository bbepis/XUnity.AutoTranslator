using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
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

      public static Coroutine Start( IEnumerator coroutine )
      {
#if IL2CPP
         var wrapper = new Il2CppSystem.Collections.IEnumerator( new Il2CppManagedEnumerator( coroutine ).Pointer );
         return PluginLoader.MonoBehaviour.StartCoroutine( wrapper );
#else
         return PluginLoader.MonoBehaviour.StartCoroutine( coroutine );
#endif

      }

      public static void Stop( Coroutine coroutine )
      {
         PluginLoader.MonoBehaviour.StopCoroutine( coroutine );
      }
   }
}
