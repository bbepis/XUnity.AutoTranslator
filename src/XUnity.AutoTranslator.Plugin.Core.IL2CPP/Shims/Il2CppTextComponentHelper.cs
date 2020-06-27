using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   internal class Il2CppTextComponentHelper : ITextComponentHelper
   {
      public bool IsSpammingComponent( object ui )
      {
         return ui is ITextComponent tc && tc.IsSpammingComponent();
      }

      public bool SupportsLineParser( object ui )
      {
         return ui is ITextComponent tc && tc.SupportsLineParser();
      }

      public bool SupportsRichText( object ui )
      {
         return ui is ITextComponent tc && tc.SupportsRichText();
      }
   }
}
