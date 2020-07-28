using XUnity.AutoTranslator.Plugin.Core.Hooks;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal class ManagedHookSetupHelper : IHookSetupHelper
   {
      public void InstallTextAssetHooks()
      {
         HooksSetup.InstallTextAssetHooks();
      }
   }
}
