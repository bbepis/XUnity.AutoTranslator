using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Extensions;

namespace XUnity.Common.Shims
{
   public class ManagedPathsHelper : IPathsHelper
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public string GameRoot => new DirectoryInfo( Application.dataPath ).Parent.FullName;
   }
}
