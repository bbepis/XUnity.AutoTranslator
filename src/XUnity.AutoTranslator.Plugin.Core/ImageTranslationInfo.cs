using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class ImageTranslationInfo
   {
      public bool IsTranslated { get; set; }

      public WeakReference<Texture2D> Original { get; private set; }

      public void Initialize( Texture2D texture )
      {
         Original = WeakReference<Texture2D>.Create( texture );
      }

      public void Reset( Texture2D newTexture )
      {
         IsTranslated = false;
         Original = WeakReference<Texture2D>.Create( newTexture );
      }
   }
}
