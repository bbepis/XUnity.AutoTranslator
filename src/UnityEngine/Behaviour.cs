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
   public class Behaviour : Component
   {
      public Behaviour() : base( IntPtr.Zero ) => throw new NotImplementedException();

      public bool enabled
      {
         get => throw new NotImplementedException();
         set => throw new NotImplementedException();
      }

      public bool isActiveAndEnabled => throw new NotImplementedException();
   }
}
