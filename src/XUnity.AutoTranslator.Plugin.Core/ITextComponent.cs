namespace XUnity.AutoTranslator.Plugin.Core
{
   internal interface ITextComponent
   {
      string text { get; set; }

      bool IsSpammingComponent();

      bool SupportsLineParser();
   }
}
