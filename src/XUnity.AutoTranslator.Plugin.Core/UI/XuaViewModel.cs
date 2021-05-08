using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   class XuaViewModel
   {
      public XuaViewModel(
         List<ToggleViewModel> toggles,
         DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager> translatorDropdown,
         DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager> fallbackDropdown,
         List<ButtonViewModel> commandButtons,
         List<LabelViewModel> labels )
      {
         Toggles = toggles;
         TranslatorDropdown = translatorDropdown;
         FallbackDropdown = fallbackDropdown;
         CommandButtons = commandButtons;
         Labels = labels;
      }

      public bool IsShown { get; set; }

      public List<ToggleViewModel> Toggles { get; }

      public DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager> TranslatorDropdown { get; }

      public DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager> FallbackDropdown { get; }

      public List<ButtonViewModel> CommandButtons { get; }

      public List<LabelViewModel> Labels { get; }
   }
}
