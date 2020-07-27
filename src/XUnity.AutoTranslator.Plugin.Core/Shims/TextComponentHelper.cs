using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   internal static class TextComponentHelper
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
            }
            return _instance;
         }
      }
   }

   internal interface ITextComponentHelper
   {
      bool IsSpammingComponent( object ui );

      bool SupportsLineParser( object ui );

      bool SupportsRichText( object ui );

      bool IsKnownTextType( object ui );

      bool SupportsStabilization( object ui );

      bool IsNGUI( object ui );

      void SetText( object ui, string text );

      string GetText( object ui );

      bool ShouldTranslateTextComponent( object ui, bool ignoreComponentState );

      bool IsComponentActive( object ui );

      TextTranslationInfo GetTextTranslationInfo( object ui );

      TextTranslationInfo GetOrCreateTextTranslationInfo( object ui );

      object CreateWrapperTextComponentIfRequiredAndPossible( object ui );

      IEnumerable<object> GetAllTextComponentsInChildren( object go );

      string[] GetPathSegments( object ui );

      string GetPath( object ui );

      bool HasIgnoredName( object ui );




      string GetTextureName( object texture, string fallbackName );

      void LoadImageEx( object texture, byte[] data, object originalTexture );

      TextureDataResult GetTextureData( object texture );

      object CreateEmptyTexture2D();


      // UI will have to be a custom implementation like ITextComponent (ITextureComponent for IL2CPP)
      bool IsKnownImageType( object ui );

      object GetTexture( object ui );

      void SetTexture( object ui, object texture );

      void SetAllDirtyEx( object ui );
   }
}
