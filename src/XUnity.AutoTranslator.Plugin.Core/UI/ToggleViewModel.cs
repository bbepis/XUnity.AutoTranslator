using System;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class ToggleViewModel
   {
      private GUIContent _enabled;
      private GUIContent _disabled;

      public ToggleViewModel( string text, string enabledTooltip, string disabledTooltip, Action onToggled, Func<bool> isToggled )
      {
         _enabled = new GUIContent( text, enabledTooltip );
         _disabled = new GUIContent( text, disabledTooltip );
         OnToggled = onToggled;
         IsToggled = isToggled;
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

      public Action OnToggled { get; set; }

      public Func<bool> IsToggled { get; set; }
   }
}
