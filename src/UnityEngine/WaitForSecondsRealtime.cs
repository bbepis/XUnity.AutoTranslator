using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   public class WaitForSecondsRealtime : CustomYieldInstruction
   {
      public float waitTime { get; set; }

      public override bool keepWaiting => Time.realtimeSinceStartup < waitTime;

      public WaitForSecondsRealtime( float time )
      {
         waitTime = Time.realtimeSinceStartup + time;
      }
   }
}
