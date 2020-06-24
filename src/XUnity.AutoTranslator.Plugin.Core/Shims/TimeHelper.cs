using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   public static class TimeHelper
   {
      private static ITimeHelper _instance;

      public static ITimeHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<ITimeHelper>(
                  "XUnity.AutoTranslator.Plugin.Core.Shims.ManagedTimeHelper, XUnity.AutoTranslator.Plugin.Core.Managed.dll",
                  "XUnity.AutoTranslator.Plugin.Core.Shims.Il2CppTimeHelper, XUnity.AutoTranslator.Plugin.Core.IL2CPP.dll" );
            }
            return _instance;
         }
      }
   }

   public interface ITimeHelper
   {
      int frameCount { get; }

      float realtimeSinceStartup { get; }

      float time { get; }

      float deltaTime { get; }
   }
}
