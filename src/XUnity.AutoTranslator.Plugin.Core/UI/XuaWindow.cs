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
      private const int WindowHeight = 480;
      private const int WindowWidth = 320;

      private const int AvailableWidth = WindowWidth - ( GUIUtil.ComponentSpacing * 2 );
      private const int AvailableHeight = WindowHeight - GUIUtil.WindowTitleClearance - ( GUIUtil.ComponentSpacing * 2 );

      private Rect _windowRect = new Rect( 20, 20, WindowWidth, WindowHeight );

      private DropdownGUI<TranslatorDropdownOptionViewModel, ConfiguredEndpoint> _endpointDropdown;

      private bool _isShown;
      private List<ToggleViewModel> _toggles;
      private List<TranslatorDropdownOptionViewModel> _endpointOptions;
      private List<ButtonViewModel> _commandButtons;
      private List<LabelViewModel> _labels;

      public bool IsShown
      {
         get
         {
            return _isShown;
         }
         set
         {
            _isShown = value;
         }
      }

      public XuaWindow(
         List<ToggleViewModel> toggles,
         List<TranslatorDropdownOptionViewModel> endpoints,
         List<ButtonViewModel> commandButtons,
         List<LabelViewModel> labels )
      {
         _toggles = toggles;
         _endpointOptions = endpoints;
         _commandButtons = commandButtons;
         _labels = labels;
      }

      public void OnGUI()
      {
         GUI.Box( _windowRect, GUIContent.none, GUIUtil.GetWindowBackgroundStyle() );

         _windowRect = GUI.Window( 5464332, _windowRect, CreateWindowUI, "---- XUnity.AutoTranslator UI ----" );
      }

      private void CreateWindowUI( int id )
      {
         int posx = GUIUtil.ComponentSpacing;
         int posy = GUIUtil.WindowTitleClearance + GUIUtil.ComponentSpacing;
         const int col2 = WindowWidth - GUIUtil.LabelWidth - ( 3 * GUIUtil.ComponentSpacing );
         const int col1x = GUIUtil.ComponentSpacing;
         const int col2x = GUIUtil.LabelWidth + ( GUIUtil.ComponentSpacing * 2 );
         const int col12 = WindowWidth - ( 2 * GUIUtil.ComponentSpacing );

         if( GUI.Button( GUIUtil.R( WindowWidth - 22, 2, 20, 16 ), "X" ) )
         {
            IsShown = false;
         }


         var halfSpacing = GUIUtil.ComponentSpacing / 2;

         // GROUP
         var groupHeight = ( GUIUtil.RowHeight * _toggles.Count ) + ( GUIUtil.ComponentSpacing * ( _toggles.Count ) ) - halfSpacing;
         GUI.Box( GUIUtil.R( halfSpacing, posy, WindowWidth - GUIUtil.ComponentSpacing, groupHeight ), "" );

         foreach( var vm in _toggles )
         {
            var previousValue = vm.IsToggled();
            var newValue = GUI.Toggle( GUIUtil.R( col1x, posy + 3, col12, GUIUtil.RowHeight - 3 ), previousValue, vm.Text );
            if( previousValue != newValue )
            {
               vm.OnToggled();
            }
            posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;
         }

         const int buttonsPerRow = 3;
         const int buttonWidth = ( col12 - ( GUIUtil.ComponentSpacing * ( buttonsPerRow - 1 ) ) ) / buttonsPerRow;
         var rows = _commandButtons.Count / buttonsPerRow;
         if( _commandButtons.Count % 3 != 0 ) rows++;

         // GROUP
         groupHeight = GUIUtil.LabelHeight + ( GUIUtil.RowHeight * rows ) + ( GUIUtil.ComponentSpacing * ( rows + 1 ) ) - halfSpacing;
         GUI.Box( GUIUtil.R( halfSpacing, posy, WindowWidth - GUIUtil.ComponentSpacing, groupHeight ), "" );

         GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.LabelHeight ), "---- Command Panel ----", GUIUtil.LabelCenter );
         posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;

         for( int row = 0 ; row < rows ; row++ )
         {
            for( int col = 0 ; col < buttonsPerRow ; col++ )
            {
               int idx = ( row * buttonsPerRow ) + col;
               if( idx >= _commandButtons.Count ) break;

               var vm = _commandButtons[ idx ];

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
         groupHeight = GUIUtil.LabelHeight + ( GUIUtil.RowHeight * 1 ) + ( GUIUtil.ComponentSpacing * ( 2 ) ) - halfSpacing;
         GUI.Box( GUIUtil.R( halfSpacing, posy, WindowWidth - GUIUtil.ComponentSpacing, groupHeight ), "" );

         GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.LabelHeight ), "---- Select a Translator ----", GUIUtil.LabelCenter );
         posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;

         GUI.Label( GUIUtil.R( col1x, posy, GUIUtil.LabelWidth, GUIUtil.LabelHeight ), "Translator: " );
         int endpointDropdownPosy = posy;
         posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;

         // GROUP
         groupHeight = GUIUtil.LabelHeight + ( GUIUtil.RowHeight * _labels.Count ) + ( GUIUtil.ComponentSpacing * ( _labels.Count + 1 ) ) - halfSpacing;
         GUI.Box( GUIUtil.R( halfSpacing, posy, WindowWidth - GUIUtil.ComponentSpacing, groupHeight ), "" );

         GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.LabelHeight ), "---- Status ----", GUIUtil.LabelCenter );
         posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;

         foreach( var label in _labels )
         {
            GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.LabelHeight ), label.Title );
            GUI.Label( GUIUtil.R( col2x, posy, col2, GUIUtil.LabelHeight ), label.GetValue(), GUIUtil.LabelRight );
            posy += GUIUtil.RowHeight + GUIUtil.ComponentSpacing;
         }

         var endpointDropdown = _endpointDropdown ?? ( _endpointDropdown = new DropdownGUI<TranslatorDropdownOptionViewModel, ConfiguredEndpoint>( col2x, endpointDropdownPosy, col2, _endpointOptions ) );
         endpointDropdown.OnGUI();

         GUI.Label( GUIUtil.R( col1x, posy, col12, GUIUtil.RowHeight * 5 ), GUI.tooltip, GUIUtil.LabelRich );

         GUI.DragWindow();
      }
   }
}
