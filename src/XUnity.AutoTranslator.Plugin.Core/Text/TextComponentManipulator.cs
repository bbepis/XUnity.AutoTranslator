namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   public class TextComponentManipulator : ITextComponentManipulator
   {
      public string GetText( object ui )
      {
         return ( (ITextComponent)ui ).Text;
      }

      public void SetText( object ui, string text )
      {
         ( (ITextComponent)ui ).Text = text;
      }
   }
}
