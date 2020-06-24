using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TextureReloadContext
   {
      private readonly HashSet<object> _textures;

      public TextureReloadContext()
      {
         _textures = new HashSet<object>();
      }

      public bool RegisterTextureInContextAndDetermineWhetherToReload( object texture )
      {
         return _textures.Add( texture );
      }
   }
}
