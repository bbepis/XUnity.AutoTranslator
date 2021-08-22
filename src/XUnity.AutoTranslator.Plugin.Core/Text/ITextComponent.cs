using System.Collections.Generic;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   internal interface ITextComponent : IGarbageCollectable
   {
      string Text { get; set; }

      bool IsSpammingComponent();

      int GetScope();

      bool SupportsRichText();

      bool IsPlaceholder();

      bool IsEnabledInSettings();

      Component Component { get; }
   }
}
