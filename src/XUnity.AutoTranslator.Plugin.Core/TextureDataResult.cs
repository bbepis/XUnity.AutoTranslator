namespace XUnity.AutoTranslator.Plugin.Core
{
   internal class TextureDataResult
   {
      public TextureDataResult( byte[] data, bool nonReadable, float calculationTime )
      {
         Data = data;
         NonReadable = nonReadable;
         CalculationTime = calculationTime;
      }

      public byte[] Data { get; }

      public bool NonReadable { get; }

      public float CalculationTime { get; set; }
   }
}
