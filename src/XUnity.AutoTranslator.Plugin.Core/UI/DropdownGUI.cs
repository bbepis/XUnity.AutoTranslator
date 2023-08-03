using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{

   internal class DropdownGUI<TDropdownOptionViewModel, TSelection>
      where TDropdownOptionViewModel : DropdownOptionViewModel<TSelection>
      where TSelection : class
   {

      private const float MaxHeight = GUIUtil.RowHeight * 5;

      private GUIContent _noSelection;
      private GUIContent _unselect;
      private DropdownViewModel<TDropdownOptionViewModel, TSelection> _viewModel;

      private float _x;
      private float _y;
      private float _width;
      private bool _isShown;
      private Vector2 _scrollPosition;

      public DropdownGUI( float x, float y, float width, DropdownViewModel<TDropdownOptionViewModel, TSelection> viewModel )
      {
         _x = x;
         _y = y;
         _width = width;
         _noSelection = GUIUtil.CreateContent( viewModel.NoSelection, viewModel.NoSelectionTooltip );
         _unselect = GUIUtil.CreateContent( viewModel.Unselect, viewModel.UnselectTooltip );

         _viewModel = viewModel;
      }

      public bool OnGUI( bool enabled )
      {
         var previouslyEnabled = GUI.enabled;

         try
         {
            GUI.enabled = enabled;

            bool clicked = GUI.Button( GUIUtil.R( _x, _y, _width, GUIUtil.RowHeight ), _viewModel.CurrentSelection?.Text ?? _noSelection, _isShown ? GUIUtil.NoMarginButtonPressedStyle : GUI.skin.button );
            if( clicked )
            {
               _isShown = !_isShown;
            }

            if( !enabled )
            {
               _isShown = false;
            }

            if( _isShown )
            {
               ShowDropdown( _x, _y + GUIUtil.RowHeight, _width, GUI.skin.button );
            }

            if( !clicked && Event.current.isMouse )
            {
               _isShown = false;
            }

            return _isShown;
         }
         finally
         {
            GUI.enabled = previouslyEnabled;
         }
      }

      private bool _supportsScrollView = true;

      private void ShowDropdown( float x, float y, float width, GUIStyle buttonStyle )
      {
         var rect = GUIUtil.R( x, y, width, _supportsScrollView && _viewModel.Options.Count * GUIUtil.RowHeight > MaxHeight ? MaxHeight : _viewModel.Options.Count * GUIUtil.RowHeight );

         GUILayout.BeginArea( rect, GUIUtil.NoSpacingBoxStyle );
         try
         {
            if( _supportsScrollView )
            {
               _scrollPosition = GUILayout.BeginScrollView( _scrollPosition, GUIStyle.none );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "GUILayout.BeginScrollView not supported. Proceeding without..." );
            _supportsScrollView = false;
         }

         var style = _viewModel.CurrentSelection == null ? GUIUtil.NoMarginButtonPressedStyle : GUIUtil.NoMarginButtonStyle;
         if( GUILayout.Button( _unselect, style, ArrayHelper.Null<GUILayoutOption>() ) )
         {
            _viewModel.Select( null );
            _isShown = false;
         }

         foreach( var option in _viewModel.Options )
         {
            style = option.IsSelected() ? GUIUtil.NoMarginButtonPressedStyle : GUIUtil.NoMarginButtonStyle;
            GUI.enabled = option?.IsEnabled() ?? true;
            if( GUILayout.Button( option.Text, style, ArrayHelper.Null<GUILayoutOption>() ) )
            {
               _viewModel.Select( option );
               _isShown = false;
            }
            GUI.enabled = true;
         }

         if( _supportsScrollView )
         {
            GUILayout.EndScrollView();
         }
         GUILayout.EndArea();
      }
   }
}
