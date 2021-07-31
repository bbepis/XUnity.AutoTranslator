namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   interface IFontAutoResizeCommand
   {
      bool ShouldAutoResize();
      double? GetMinSize();
      double? GetMaxSize();
   }
}
