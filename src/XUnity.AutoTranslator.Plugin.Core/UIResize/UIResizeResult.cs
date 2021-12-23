namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UIResizeResult
   {
      public IFontResizeCommand ResizeCommand { get; set; }
      public bool IsResizeCommandScoped { get; set; }

      public IFontAutoResizeCommand AutoResizeCommand { get; set; }
      public bool IsAutoResizeCommandScoped { get; set; }

      public IUGUI_LineSpacingCommand LineSpacingCommand { get; set; }
      public bool IsLineSpacingCommandScoped { get; set; }

      public IUGUI_HorizontalOverflow HorizontalOverflowCommand { get; set; }
      public bool IsHorizontalOverflowCommandScoped { get; set; }

      public IUGUI_VerticalOverflow VerticalOverflowCommand { get; set; }
      public bool IsVerticalOverflowCommandScoped { get; set; }

      public ITMP_OverflowMode OverflowCommand { get; set; }
      public bool IsOverflowCommandScoped { get; set; }

      public ITMP_Alignment AlignmentCommand { get; set; }
      public bool IsAlignmentCommandScoped { get; set; }

      public bool IsEmpty()
      {
         return ResizeCommand == null
            && AutoResizeCommand == null
            && LineSpacingCommand == null
            && HorizontalOverflowCommand == null
            && VerticalOverflowCommand == null
            && OverflowCommand == null
            && AlignmentCommand == null;
      }

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
            IsResizeCommandScoped = otherResult.IsResizeCommandScoped;
         }

         if( otherResult.AutoResizeCommand != null && ( otherResult.IsAutoResizeCommandScoped || ( !otherResult.IsAutoResizeCommandScoped && !IsAutoResizeCommandScoped ) ) )
         {
            AutoResizeCommand = otherResult.AutoResizeCommand;
            IsAutoResizeCommandScoped = otherResult.IsAutoResizeCommandScoped;
         }

         if( otherResult.LineSpacingCommand != null && ( otherResult.IsLineSpacingCommandScoped || ( !otherResult.IsLineSpacingCommandScoped && !IsLineSpacingCommandScoped ) ) )
         {
            LineSpacingCommand = otherResult.LineSpacingCommand;
            IsLineSpacingCommandScoped = otherResult.IsLineSpacingCommandScoped;
         }

         if( otherResult.HorizontalOverflowCommand != null && ( otherResult.IsHorizontalOverflowCommandScoped || ( !otherResult.IsHorizontalOverflowCommandScoped && !IsHorizontalOverflowCommandScoped ) ) )
         {
            HorizontalOverflowCommand = otherResult.HorizontalOverflowCommand;
            IsHorizontalOverflowCommandScoped = otherResult.IsHorizontalOverflowCommandScoped;
         }

         if( otherResult.VerticalOverflowCommand != null && ( otherResult.IsVerticalOverflowCommandScoped || ( !otherResult.IsVerticalOverflowCommandScoped && !IsVerticalOverflowCommandScoped ) ) )
         {
            VerticalOverflowCommand = otherResult.VerticalOverflowCommand;
            IsVerticalOverflowCommandScoped = otherResult.IsVerticalOverflowCommandScoped;
         }

         if( otherResult.OverflowCommand != null && ( otherResult.IsOverflowCommandScoped || ( !otherResult.IsOverflowCommandScoped && !IsOverflowCommandScoped ) ) )
         {
            OverflowCommand = otherResult.OverflowCommand;
            IsOverflowCommandScoped = otherResult.IsOverflowCommandScoped;
         }

         if( otherResult.AlignmentCommand != null && ( otherResult.IsAlignmentCommandScoped || ( !otherResult.IsAlignmentCommandScoped && !IsAlignmentCommandScoped ) ) )
         {
            AlignmentCommand = otherResult.AlignmentCommand;
            IsAlignmentCommandScoped = otherResult.IsAlignmentCommandScoped;
         }
      }
   }
}
