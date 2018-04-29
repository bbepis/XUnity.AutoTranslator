using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Setup
{
   public class GameLauncher
   {
      public GameLauncher( FileInfo executable, DirectoryInfo data )
      {
         Executable = executable;
         Data = data;
      }

      public FileInfo Executable { get; private set; }

      public DirectoryInfo Data { get; private set; }
   }
}
