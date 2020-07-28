using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Support;

namespace XUnity.AutoTranslator.Plugin.Core.Managed.Shims
{
   internal class ManagedTimeHelper : ITimeHelper
   {
      public int frameCount => Time.frameCount;

      public float realtimeSinceStartup => Time.realtimeSinceStartup;

      public float time => Time.time;

      public float deltaTime => Time.deltaTime;
   }
}
