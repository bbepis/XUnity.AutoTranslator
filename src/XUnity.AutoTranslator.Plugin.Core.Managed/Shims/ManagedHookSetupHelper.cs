using XUnity.AutoTranslator.Plugin.Core.Hooks;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   internal class ManagedHookSetupHelper : IHookSetupHelper
   {
      public void InstallTextAssetHooks()
      {
         HooksSetup.InstallTextAssetHooks();
      }
   }
}
