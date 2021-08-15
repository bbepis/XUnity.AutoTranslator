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
   public sealed class AssetBundleCreateRequest : AsyncOperation
   {
      public AssetBundle assetBundle
      {
         get;
      }

      internal extern void DisableCompatibilityChecks();
   }
}
