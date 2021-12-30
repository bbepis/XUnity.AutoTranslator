using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Managed.Textures;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Textures
{
   internal static class TextureLoader
   {
      private static readonly Dictionary<ImageFormat, ITextureLoader> Loaders
         = new Dictionary<ImageFormat, ITextureLoader>();

      static TextureLoader()
      {
         _ = Register( ImageFormat.PNG, new LoadImageImageLoader() );
         _ = Register( ImageFormat.TGA, new TgaImageLoader() ) || Register( ImageFormat.TGA, new FallbackTgaImageLoader() );
      }

      public static bool Register( ImageFormat format, ITextureLoader loader )
      {
         try
         {
            var verified = loader.Verify();
            if( verified )
            {
               Loaders[ format ] = loader;
               return true;
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An image loader could not be registered." );
         }

         return false;
      }

      public static void Load( Texture2D texture, byte[] data, ImageFormat imageFormat )
      {
         if( Loaders.TryGetValue( imageFormat, out var loader ) )
         {
            loader.Load( texture, data );
         }
      }
   }
}
