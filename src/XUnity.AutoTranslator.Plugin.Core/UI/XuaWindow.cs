using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class XuaWindow
   {
      private const int WindowId = 5464332;
      private const float WindowHeight = 596;
      private const float WindowWidth = 320;

      private Rect _windowRect = new Rect( 20, 20, WindowWidth, WindowHeight );

      private DropdownGUI<TranslatorDropdownOptionViewModel, TranslationEndpointManager> _endpointDropdown;
      private DropdownGUI<TranslatorDropdownOptionViewModel, TranslationEndpointManager> _fallbackDropdown;
      private XuaViewModel _viewModel;
      private bool _isMouseDownOnWindow = false;

      public bool IsShown
      {
         get => _viewModel.IsShown;
         set => _viewModel.IsShown = value;
      }

      public XuaWindow( XuaViewModel viewModel )
      {
         _viewModel = viewModel;
      }

      public void OnGUI()
      {
         GUI.Box( _windowRect, GUIUtil.none, GUIUtil.GetWindowBackgroundStyle() );

         _windowRect = GUI.Window( WindowId, _windowRect, (GUI.WindowFunction)CreateWindowUI, "---- XUnity.AutoTranslator UI ----" );

         if( GUIUtil.IsAnyMouseButtonOrScrollWheelDownSafe )
         {
            var point = new Vector2( Input.mousePosition.x, Screen.height - Input.mousePosition.y );
            _isMouseDownOnWindow = _windowRect.Contains( point );
         }

         if( !_isMouseDownOnWindow || !GUIUtil.IsAnyMouseButtonOrScrollWheelSafe )
            return;

         // make sure window is focused if scroll wheel is used to indicate we consumed that event
         GUI.FocusWindow( WindowId );

         var point1 = new Vector2( Input.mousePosition.x, Screen.height - Input.mousePosition.y );
         if( !_windowRect.Contains( point1 ) )
            return;

         Input.ResetInputAxes();
      }
      
      private void CreateWindowUI( int id )
      {
         try
         {
            AutoTranslationPlugin.Current.DisableAutoTranslator();

            float posx = GUIUtil.ComponentSpacing;
            float posy = GUIUtil.WindowTitleClearance + GUIUtil.ComponentSpacing;
            const float col2 = WindowWidth - GUIUtil.LabelWidth - ( 3 * GUIUtil.ComponentSpacing );
            const float col1x = GUIUtil.ComponentSpacing;
            const float col2x = GUIUtil.LabelWidth + ( GUIUtil.ComponentSpacing * 2 );
            const float col12 = WindowWidth - ( 2 * GUIUtil.ComponentSpacing );

            if( GUI.Button( GUIUtil.R( WindowWidth - 22, 2, 20, 16 ), "X" ) )
            {
               IsShown = false;
            }

            // GROUP
            var toggles = _viewModel.Toggles;
            var groupHeight = ( GUIUtil.RowHeight * toggles.Count ) + ( GUIUtil.HalfComponentSpacing * toggles.Count ) - GUIUtil.ComponentSpacing;
            GUI.Box( GUIUtil.R( GUIUtil.HalfComponentSpacing, posy, WindowWidth - GUIUtil.ComponentSpacing, groupHeight ), "" );

            foreach( var vm in toggles )
            {
               var previousValue = vm.IsToggled();
               var newValue = GUI.Toggle( GUIUtil.R( col1x, posy + 3, col12, GUIUtil.RowHeight - 3 ), previousValue, vm.Text );
               if( previousValue != newValue )
               {
                  vm.OnToggled();
               }
               posy += GUIUtil.RowHeight;
            }
            posy += GUIUtil.ComponentSpacing;

            var commandButtons = _viewModel.CommandButtons;
            const int buttonsPerRow = 3;
            const float buttonWidth = ( col12 - ( GUIUtil.ComponentSpacing * ( buttonsPerRow - 1 ) ) ) / buttonsPerRow;
            var rows = commandButtons.Count / buttonsPerRow;
            if( commandButtons.Count % 3 != 0 ) rows++;

            // GROUP
            groupHeight = GUIUtil.LabelHeight + ( GUIUtil.RowHeight * rows ) + ( GUIUtil.ComponentSpacing * ( rows + 1 ) ) - GUIUtil.HalfComponentSpacing;
            GUI.Box( GUIUtil.R( GUIUtil.HalfComponentSpacing, posy, WindowWidth - GUIUtil.ComponentSpacing, groupHeight ), "" );

            GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.LabelHeight ), "---- Command Panel ----", GUIUtil.LabelCenter );
            posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;

            for( int row = 0; row < rows; row++ )
            {
               for( int col = 0; col < buttonsPerRow; col++ )
               {
                  int idx = ( row * buttonsPerRow ) + col;
                  if( idx >= commandButtons.Count ) break;

                  var vm = commandButtons[ idx ];

                  GUI.enabled = vm.CanClick?.Invoke() != false;
                  if( GUI.Button( GUIUtil.R( posx, posy, buttonWidth, GUIUtil.RowHeight ), vm.Text ) )
                  {
                     vm.OnClicked?.Invoke();
                  }
                  GUI.enabled = true;

                  posx += GUIUtil.ComponentSpacing + buttonWidth;
               }
               posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;
            }

            // GROUP
            groupHeight = GUIUtil.LabelHeight + ( GUIUtil.RowHeight * 2 ) + ( GUIUtil.ComponentSpacing * 2 );
            GUI.Box( GUIUtil.R( GUIUtil.HalfComponentSpacing, posy, WindowWidth - GUIUtil.ComponentSpacing, groupHeight ), "" );

            GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.LabelHeight ), "---- Select a Translator ----", GUIUtil.LabelCenter );
            posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;

            GUI.Label( GUIUtil.R( col1x, posy, GUIUtil.LabelWidth + 10, GUIUtil.LabelHeight ), "Translator: " );
            float endpointDropdownPosy = posy;
            posy += GUIUtil.RowHeight + GUIUtil.HalfComponentSpacing;

            GUI.Label( GUIUtil.R( col1x, posy, GUIUtil.LabelWidth, GUIUtil.LabelHeight ), "Fallback: " );
            float fallbackDropdownPosy = posy;
            posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;

            // GROUP
            var labels = _viewModel.Labels;
            groupHeight = GUIUtil.LabelHeight + ( GUIUtil.RowHeight * labels.Count ) + ( GUIUtil.ComponentSpacing * ( labels.Count + 1 ) ) - GUIUtil.HalfComponentSpacing;
            GUI.Box( GUIUtil.R( GUIUtil.HalfComponentSpacing, posy, WindowWidth - GUIUtil.ComponentSpacing, groupHeight ), "" );

            GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.LabelHeight ), "---- Status ----", GUIUtil.LabelCenter );
            posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;

            foreach( var label in labels )
            {
               GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.LabelHeight ), label.Title );
               GUI.Label( GUIUtil.R( col2x, posy, col2, GUIUtil.LabelHeight ), label.GetValue(), GUIUtil.LabelRight );
               posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;
            }

            var endpointDropdown = _endpointDropdown ?? ( _endpointDropdown = new DropdownGUI<TranslatorDropdownOptionViewModel, TranslationEndpointManager>( col2x, endpointDropdownPosy, col2, _viewModel.TranslatorDropdown ) );
            var isShown = endpointDropdown.OnGUI(true);

            var fallbackDropdown = _fallbackDropdown ?? ( _fallbackDropdown = new DropdownGUI<TranslatorDropdownOptionViewModel, TranslationEndpointManager>( col2x, fallbackDropdownPosy, col2, _viewModel.FallbackDropdown ) );
            fallbackDropdown.OnGUI( !isShown );

            GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.RowHeight * 5 ), GUI.tooltip, GUIUtil.LabelRich );

            GUI.DragWindow();
         }
         finally
         {
            AutoTranslationPlugin.Current.EnableAutoTranslator();
         }
      }
   }
}
