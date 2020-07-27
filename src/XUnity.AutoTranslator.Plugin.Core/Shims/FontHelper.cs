using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   internal static class FontHelper
   {
      private static IFontHelper _instance;

      public static IFontHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<IFontHelper>(
                  typeof( FontHelper ).Assembly,
                  "XUnity.AutoTranslator.Plugin.Core.Managed.dll",
                  "XUnity.AutoTranslator.Plugin.Core.IL2CPP.dll" );
            }
            return _instance;
         }
      }
   }

   internal interface IFontHelper
   {
      object GetTextMeshProFont();

      object GetTextFont( int size );

      string[] GetOSInstalledFontNames();
   }
}
