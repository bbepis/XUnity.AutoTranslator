namespace XUnity.AutoTranslator.Plugin.Core.Textures
{
   internal interface ITextureLoader
   {
      void Load( object texture, byte[] data );

      bool Verify();
   }
}
