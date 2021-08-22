using System;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class LabelViewModel
   {
      public LabelViewModel( string title, Func<string> getValue )
      {
         Title = title;
         GetValue = getValue;
      }

      public string Title { get; set; }

      public Func<string> GetValue { get; set; }
   }
}
