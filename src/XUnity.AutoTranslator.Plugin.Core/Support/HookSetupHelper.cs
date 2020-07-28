using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal static class HookSetupHelper
   {
      private static IHookSetupHelper _instance;

      public static IHookSetupHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<IHookSetupHelper>(
                  typeof( HookSetupHelper ).Assembly,
                  "XUnity.AutoTranslator.Plugin.Core.Managed.dll",
                  "XUnity.AutoTranslator.Plugin.Core.IL2CPP.dll" );
            }
            return _instance;
         }
      }
   }

   internal interface IHookSetupHelper
   {
      void InstallTextAssetHooks();
   }
}
