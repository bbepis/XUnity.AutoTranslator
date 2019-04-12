using System;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class DropdownOptionViewModel<TSelection>
   {
      public DropdownOptionViewModel( string text, Func<bool> isSelected, Func<bool> isEnabled, TSelection selection, Action<TSelection> onSelected )
      {
         Text = new GUIContent( text );
         IsSelected = isSelected;
         IsEnabled = isEnabled;
         Selection = selection;
         OnSelected = onSelected;
      }

      public virtual GUIContent Text { get; set; }

      public Func<bool> IsEnabled { get; set; }

      public Func<bool> IsSelected { get; set; }

      public TSelection Selection { get; set; }

      public Action<TSelection> OnSelected { get; set; }
   }

   internal class TranslatorDropdownOptionViewModel : DropdownOptionViewModel<TranslationEndpointManager>
   {
      private GUIContent _selected;
      private GUIContent _normal;
      private GUIContent _disabled;

      public TranslatorDropdownOptionViewModel( Func<bool> isSelected, TranslationEndpointManager selection, Action<TranslationEndpointManager> onSelected ) : base( selection.Endpoint.FriendlyName, isSelected, () => selection.Error == null, selection, onSelected )
      {
         _selected = new GUIContent( selection.Endpoint.FriendlyName, $"<b>CURRENT TRANSLATOR</b>\n{selection.Endpoint.FriendlyName} is the currently selected translator that will be used to perform translations." );
         _disabled = new GUIContent( selection.Endpoint.FriendlyName, $"<b>CANNOT SELECT TRANSLATOR</b>\n{selection.Endpoint.FriendlyName} cannot be selected because the initialization failed. {selection.Error?.Message}" );
         _normal = new GUIContent( selection.Endpoint.FriendlyName, $"<b>SELECT TRANSLATOR</b>\n{selection.Endpoint.FriendlyName} will be selected as translator." );
      }

      public override GUIContent Text
      {
         get
         {
            if( Selection.Error != null )
            {
               return _disabled;
            }
            else if( IsSelected() )
            {
               return _selected;
            }
            else
            {
               return _normal;
            }
         }
      }
   }
}
