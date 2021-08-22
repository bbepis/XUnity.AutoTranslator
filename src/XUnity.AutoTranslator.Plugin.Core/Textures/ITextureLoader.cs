using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Textures
{
   internal interface ITextureLoader
   {
      void Load( Texture2D texture, byte[] data );

      bool Verify();
   }
}
