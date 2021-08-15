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
   public enum ScreenOrientation
   {
      Unknown = 0,
      Portrait = 1,
      PortraitUpsideDown = 2,
      LandscapeLeft = 3,
      LandscapeRight = 4,
      AutoRotation = 5,
      Landscape = 3
   }
}
