using System.Collections.Generic;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   internal interface ITextComponent : IGarbageCollectable
   {
      string text { get; set; }

      bool IsSpammingComponent();

      int GetScope();

      bool SupportsRichText();

      bool IsPlaceholder();

      object Component { get; }
   }
}
