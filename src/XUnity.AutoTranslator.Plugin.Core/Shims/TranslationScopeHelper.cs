using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Shims
{
   public static class TranslationScopeHelper
   {
      private static ITranslationScopeHelper _instance;

      public static ITranslationScopeHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<ITranslationScopeHelper>(
                  "XUnity.AutoTranslator.Plugin.Core.Shims.ManagedTranslationScopeHelper, XUnity.AutoTranslator.Plugin.Core.Managed.dll",
                  "XUnity.AutoTranslator.Plugin.Core.Shims.Il2CppTranslationScopeHelper, XUnity.AutoTranslator.Plugin.Core.IL2CPP.dll" );
            }
            return _instance;
         }
      }
   }

   public interface ITranslationScopeHelper
   {
      int GetScope( object ui );

      int GetScopeFromComponent( object component );

      bool SupportsSceneManager();

      int GetActiveSceneId();
   }
}
