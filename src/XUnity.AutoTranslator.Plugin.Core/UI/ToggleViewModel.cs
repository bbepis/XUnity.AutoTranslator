using System;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class ToggleViewModel
   {
      private GUIContent _enabled;
      private GUIContent _disabled;

      public ToggleViewModel( string text, string enabledTooltip, string disabledTooltip, Action onToggled, Func<bool> isToggled, bool enabled = true )
      {
         _enabled = GUIUtil.CreateContent( text, enabledTooltip );
         _disabled = GUIUtil.CreateContent( text, disabledTooltip );
         OnToggled = onToggled;
         IsToggled = isToggled;
         Enabled = enabled;
      }

      public GUIContent Text
      {
         get
         {
            if( IsToggled() )
            {
               return _enabled;
            }
            return _disabled;
         }
      }

      public bool Enabled { get; set; }

      public Action OnToggled { get; set; }

      public Func<bool> IsToggled { get; set; }
   }
}
