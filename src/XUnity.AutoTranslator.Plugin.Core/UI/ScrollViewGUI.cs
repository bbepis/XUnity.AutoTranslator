using System;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class ScrollPositioned
   {
      public ScrollPositioned()
      {
      }

      public Vector2 ScrollPosition { get; set; }
   }

   internal class ScrollPositioned<TViewModel> : ScrollPositioned
   {
      public ScrollPositioned( TViewModel viewModel )
      {
         ViewModel = viewModel;
      }

      public TViewModel ViewModel { get; private set; }
   }
}
