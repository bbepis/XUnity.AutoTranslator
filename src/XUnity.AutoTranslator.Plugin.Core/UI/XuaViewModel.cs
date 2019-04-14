using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class XuaViewModel
   {
      public XuaViewModel(
         List<ToggleViewModel> toggles,
         List<TranslatorDropdownOptionViewModel> endpoints,
         List<ButtonViewModel> commandButtons,
         List<LabelViewModel> labels )
      {
         Toggles = toggles;
         EndpointOptions = endpoints;
         CommandButtons = commandButtons;
         Labels = labels;
      }

      public bool IsShown { get; set; }

      public List<ToggleViewModel> Toggles { get; }

      public List<TranslatorDropdownOptionViewModel> EndpointOptions { get; }

      public List<ButtonViewModel> CommandButtons { get; }

      public List<LabelViewModel> Labels { get; }
   }
}
