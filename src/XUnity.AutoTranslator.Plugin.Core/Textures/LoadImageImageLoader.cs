using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Textures;
using XUnity.Common.Constants;
using XUnity.Common.Harmony;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Managed.Textures
{
   class LoadImageImageLoader : ITextureLoader
   {
      public void Load( Texture2D texture, byte[] data )
      {
         if( UnityTypes.ImageConversion_Methods.LoadImage != null )
         {
            UnityTypes.ImageConversion_Methods.LoadImage( texture, data, false );
         }
         else if( UnityTypes.Texture2D_Methods.LoadImage != null )
         {
            UnityTypes.Texture2D_Methods.LoadImage( texture, data );
         }
      }

      public bool Verify()
      {
         return UnityTypes.Texture2D_Methods.LoadImage != null
            || UnityTypes.ImageConversion_Methods.LoadImage != null;
      }
   }
}
