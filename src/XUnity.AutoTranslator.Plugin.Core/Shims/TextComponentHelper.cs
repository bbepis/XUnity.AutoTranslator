using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   public static class TextComponentHelper
   {
      private static ITextComponentHelper _instance;

      public static ITextComponentHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<ITextComponentHelper>(
                  typeof( TextComponentHelper ).Assembly,
                  "XUnity.AutoTranslator.Plugin.Core.Managed.dll",
                  "XUnity.AutoTranslator.Plugin.Core.IL2CPP.dll" );

               System.Console.WriteLine( "Instantiated: " + _instance?.GetType().Name );
            }
            return _instance;
         }
      }
   }

   public interface ITextComponentHelper
   {
      bool IsSpammingComponent( object ui );

      bool SupportsLineParser( object ui );

      bool SupportsRichText( object ui );
   }
}
