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
#if IL2CPP
   public sealed class GUILayoutOption : UnhollowerBaseLib.Il2CppObjectBase
#else
   public sealed class GUILayoutOption
#endif
   {
      internal enum Type
      {
         fixedWidth,
         fixedHeight,
         minWidth,
         maxWidth,
         minHeight,
         maxHeight,
         stretchWidth,
         stretchHeight,
         alignStart,
         alignMiddle,
         alignEnd,
         alignJustify,
         equalSize,
         spacing
      }

      internal Type type;

      internal object value;

#if IL2CPP
      internal GUILayoutOption( Type type, object value ) : base( IntPtr.Zero ) => throw new NotImplementedException();
#else
      internal GUILayoutOption( Type type, object value ) => throw new NotImplementedException();
#endif

#if IL2CPP
      public GUILayoutOption( IntPtr pointer ) : base( pointer ) => throw new NotImplementedException();
#endif
   }
}
