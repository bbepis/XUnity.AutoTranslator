using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Utilities;

namespace XUnity.Common.Shims
{
   public static class PathsHelper
   {
      private static IPathsHelper _instance;

      public static IPathsHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<IPathsHelper>(
                  "XUnity.Common.Shims.ManagedPathsHelper, XUnity.Common.Managed.dll",
                  "XUnity.Common.Shims.Il2CppPathsHelper, XUnity.Common.IL2CPP.dll" );
            }
            return _instance;
         }
      }
   }

   public interface IPathsHelper
   {
      string GameRoot { get; }
   }
}
