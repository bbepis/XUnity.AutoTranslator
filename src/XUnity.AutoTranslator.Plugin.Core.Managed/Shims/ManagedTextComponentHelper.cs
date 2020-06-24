using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Shims
{
   internal class ManagedTextComponentHelper : ITextComponentHelper
   {
      public bool IsSpammingComponent( object ui )
      {
         return ui.IsSpammingComponent();
      }

      public bool SupportsLineParser( object ui )
      {
         return ui.SupportsLineParser();
      }

      public bool SupportsRichText( object ui )
      {
         return ui.SupportsRichText();
      }
   }
}
