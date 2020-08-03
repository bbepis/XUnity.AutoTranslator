using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Textures;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;

namespace XUnity.AutoTranslator.Plugin.Core.Managed.Textures
{
   class LoadImageImageLoader : ITextureLoader
   {
      private static readonly MethodInfo ImageConversion_LoadImage =
         UnityTypes.ImageConversion != null
         ? AccessToolsShim.Method( UnityTypes.ImageConversion, "LoadImage", new[] { typeof( Texture2D ), typeof( byte[] ), typeof( bool ) } )
         : null;
      private static readonly MethodInfo Texture2D_LoadImage =
         UnityTypes.Texture != null
         ? AccessToolsShim.Method( UnityTypes.Texture2D, "LoadImage", new[] { typeof( byte[] ) } )
         : null;

      public void Load( object texture, byte[] data )
      {
         if( ImageConversion_LoadImage != null )
         {
            ImageConversion_LoadImage.Invoke( null, new object[] { texture, data, false } );
         }
         else
         {
            LoadImageSafe( (Texture2D)texture, data );
         }
      }

      public bool Verify()
      {
         return ImageConversion_LoadImage != null || Texture2D_LoadImage != null;
      }

      private static void LoadImageSafe( Texture2D texture, byte[] data )
      {
         texture.LoadImage( data );
      }
   }
}
