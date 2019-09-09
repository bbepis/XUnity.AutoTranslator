namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UIResizeResult
   {
      public IFontResizeCommand ResizeCommand { get; set; }
      public bool IsResizeCommandScoped { get; set; }

      public IFontAutoResizeCommand AutoResizeCommand { get; set; }
      public bool IsAutoResizeCommandScoped { get; set; }

      public ILineSpacingCommand LineSpacingCommand { get; set; }
      public bool IsLineSpacingCommandScoped { get; set; }

      public UIResizeResult Copy()
      {
         return (UIResizeResult)MemberwiseClone();
      }

      public void MergeInto( UIResizeResult otherResult )
      {
         if( otherResult == null ) return;

         if( otherResult.ResizeCommand != null && ( otherResult.IsResizeCommandScoped || ( !otherResult.IsResizeCommandScoped && !IsResizeCommandScoped ) ) )
         {
            ResizeCommand = otherResult.ResizeCommand;
         }

         if( otherResult.AutoResizeCommand != null && ( otherResult.IsAutoResizeCommandScoped || ( !otherResult.IsAutoResizeCommandScoped && !IsAutoResizeCommandScoped ) ) )
         {
            AutoResizeCommand = otherResult.AutoResizeCommand;
         }

         if( otherResult.LineSpacingCommand != null && ( otherResult.IsLineSpacingCommandScoped || ( !otherResult.IsLineSpacingCommandScoped && !IsLineSpacingCommandScoped ) ) )
         {
            LineSpacingCommand = otherResult.LineSpacingCommand;
         }
      }
   }
}
