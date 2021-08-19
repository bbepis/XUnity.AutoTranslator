using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal static class ComponentHelper
   {
      private static IComponentHelper _instance;

      public static IComponentHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<IComponentHelper>(
                  typeof( ComponentHelper ).Assembly,
                  "XUnity.AutoTranslator.Plugin.Core.Managed.dll",
                  "XUnity.AutoTranslator.Plugin.Core.IL2CPP.dll" );
            }
            return _instance;
         }
      }
   }

   internal interface IComponentHelper
   {
      bool IsSpammingComponent( object ui );

      bool SupportsLineParser( object ui );

      bool SupportsRichText( object ui );

      bool IsKnownTextType( object ui );

      bool SupportsStabilization( object ui );

      bool IsNGUI( object ui );

      void SetText( object ui, string text );

      string GetText( object ui );

      bool IsComponentActive( object ui );

      TextTranslationInfo GetTextTranslationInfo( object ui );

      TextTranslationInfo GetOrCreateTextTranslationInfo( object ui );

      object CreateWrapperTextComponentIfRequiredAndPossible( object ui );

      IEnumerable<object> GetAllTextComponentsInChildren( object go );

      string[] GetPathSegments( object ui );

      string GetPath( object ui );

      bool HasIgnoredName( object ui );

      string GetTextureName( object texture, string fallbackName );

      void LoadImageEx( object texture, byte[] data, ImageFormat dataType, object originalTexture );

      TextureDataResult GetTextureData( object texture );

      object CreateEmptyTexture2D( int originalTextureFormat );

      bool IsCompatible( object texture, ImageFormat dataType );


      // UI will have to be a custom implementation like ITextComponent (ITextureComponent for IL2CPP)
      bool IsKnownImageType( object ui );

      object GetTexture( object ui );

      object SetTexture( object ui, object textureObj, object spriteObj, bool isPrefixHooked );

      void SetAllDirtyEx( object ui );

      int GetScreenWidth();

      int GetScreenHeight();
   }
}
