using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class Paths
   {
      private static string _gameRoot;

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static string GameRoot
      {
         get => _gameRoot ?? GetAndSetGameRoot();
         set => _gameRoot = value;
      }

      public static void Initialize()
      {
         GetAndSetGameRoot();
      }

      private static string GetAndSetGameRoot()
      {
         return _gameRoot = new DirectoryInfo( Application.dataPath ).Parent.FullName;
      }
   }
}
