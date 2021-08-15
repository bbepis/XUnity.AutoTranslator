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
   [StructLayout( LayoutKind.Sequential )]
   public sealed class ResourceRequest : AsyncOperation
   {
      internal string m_Path;

      internal Type m_Type;

      public Object asset => Resources.Load( m_Path, m_Type );
   }
}
