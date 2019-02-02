using System.Collections.Generic;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TextureReloadContext
   {
      private readonly HashSet<Texture2D> _textures;

      public TextureReloadContext()
      {
         _textures = new HashSet<Texture2D>();
      }

      public bool RegisterTextureInContextAndDetermineWhetherToReload( Texture2D texture )
      {
         return _textures.Add( texture );
      }
   }
}
