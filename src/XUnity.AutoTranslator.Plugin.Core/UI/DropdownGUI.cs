using System.Collections.Generic;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class DropdownGUI<TDropdownOptionViewModel, TSelection>
      where TDropdownOptionViewModel : DropdownOptionViewModel<TSelection>
   {

      private const int MaxHeight = GUIUtil.RowHeight * 5;

      private GUIContent _noSelection;
      private List<TDropdownOptionViewModel> _options;
      private TDropdownOptionViewModel _currentSelection;

      private int _x;
      private int _y;
      private int _width;
      private bool _isShown;
      private Vector2 _scrollPosition;

      public DropdownGUI( int x, int y, int width, IEnumerable<TDropdownOptionViewModel> options )
      {
         _x = x;
         _y = y;
         _width = width;
         _noSelection = new GUIContent( "----", "<b>SELECT TRANSLATOR</b>\nNo translator is currently selected, which means no new translations will be performed. Please select one from the dropdown." );

         _options = new List<TDropdownOptionViewModel>();
         foreach( var item in options )
         {
            if( item.IsSelected() )
            {
               _currentSelection = item;
            }
            _options.Add( item );
         }
      }

      public void Select( TDropdownOptionViewModel option )
      {
         if( option.IsSelected() ) return;

         _currentSelection = option;
         _currentSelection.OnSelected?.Invoke( _currentSelection.Selection );
      }

      public void OnGUI()
      {
         bool clicked = GUI.Button( GUIUtil.R( _x, _y, _width, GUIUtil.RowHeight ), _currentSelection?.Text ?? _noSelection, _isShown ? GUIUtil.NoMarginButtonPressedStyle : GUI.skin.button );
         if( clicked )
         {
            _isShown = !_isShown;
         }

         if( _isShown )
         {
            _scrollPosition = ShowDropdown( _x, _y + GUIUtil.RowHeight, _width, GUI.skin.button, _scrollPosition );
         }

         if( !clicked && Event.current.isMouse )
         {
            _isShown = false;
         }
      }

      private Vector2 ShowDropdown( int x, int y, int width, GUIStyle buttonStyle, Vector2 scrollPosition )
      {
         var rect = GUIUtil.R( x, y, width, _options.Count * GUIUtil.RowHeight > MaxHeight ? MaxHeight : _options.Count * GUIUtil.RowHeight );

         GUILayout.BeginArea( rect, GUIUtil.NoSpacingBoxStyle );
         scrollPosition = GUILayout.BeginScrollView( scrollPosition );

         foreach( var option in _options )
         {
            var style = option.IsSelected() ? GUIUtil.NoMarginButtonPressedStyle : GUIUtil.NoMarginButtonStyle;

            GUI.enabled = option?.IsEnabled() ?? true;
            if( GUILayout.Button( option.Text, style ) )
            {
               Select( option );
               _isShown = false;
            }
            GUI.enabled = true;
         }

         GUILayout.EndScrollView();
         GUILayout.EndArea();

         return scrollPosition;
      }
   }
}
