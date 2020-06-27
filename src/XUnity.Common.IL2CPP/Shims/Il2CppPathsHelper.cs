using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.Common.Shims
{
   public class Il2CppPathsHelper : IPathsHelper
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public string GameRoot => new DirectoryInfo( UnityEngine.Application.dataPath ).Parent.FullName;
   }
}
