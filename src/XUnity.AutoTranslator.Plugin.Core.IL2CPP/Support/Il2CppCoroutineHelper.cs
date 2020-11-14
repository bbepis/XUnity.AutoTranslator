using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.IL2CPP;
using XUnity.AutoTranslator.Plugin.Core.Support;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal class Il2CppCoroutineHelper : ICoroutineHelper
   {
      public object CreateWaitForSeconds( float seconds )
      {
         return new WaitForSeconds( seconds );
      }

      public object CreateWaitForSecondsRealtime( float delay )
      {
         return new WaitForSecondsRealtime( delay );
      }

      public object Start( IEnumerator coroutine )
      {
         return Il2CppCoroutines.Start( coroutine );
      }

      public void Stop( object coroutine )
      {
         Il2CppCoroutines.Stop( (IEnumerator)coroutine );
      }

      public bool SupportsCustomYieldInstruction()
      {
         return true;
      }
   }
}
