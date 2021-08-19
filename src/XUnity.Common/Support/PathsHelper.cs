using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.Common.Support
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class PathsHelper
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static string GameRoot => new DirectoryInfo( Application.dataPath ).Parent.FullName;
   }
}
