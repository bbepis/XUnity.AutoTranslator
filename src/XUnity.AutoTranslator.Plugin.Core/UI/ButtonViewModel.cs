using System;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class ButtonViewModel
   {
      public ButtonViewModel( string text, string tooltip, Action onClicked, Func<bool> canClick )
      {
         Text = GUIUtil.CreateContent( text, tooltip );
         OnClicked = onClicked;
         CanClick = canClick;
      }

      public GUIContent Text { get; set; }

      public Action OnClicked { get; set; }

      public Func<bool> CanClick { get; set; }
   }
}
