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
   public enum AudioType
   {
      UNKNOWN = 0,
      ACC = 1,
      AIFF = 2,
      IT = 10,
      MOD = 12,
      MPEG = 13,
      OGGVORBIS = 14,
      S3M = 17,
      WAV = 20,
      XM = 21,
      XMA = 22,
      VAG = 23,
      AUDIOQUEUE = 24
   }
}
