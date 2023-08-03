using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class TranslationAggregatorWindow
   {
      private static string[] Empty = new string[ 0 ];

      private const int WindowId = 2387602;

      private Rect _windowRect;
      private bool _isMouseDownOnWindow = false;
      private TranslationAggregatorViewModel _viewModel;

      private ScrollPositioned _originalText;
      private ScrollPositioned _defaultTranslation;
      private ScrollPositioned<TranslatorViewModel>[] _translationViews;

      public TranslationAggregatorWindow( TranslationAggregatorViewModel viewModel )
      {
         _viewModel = viewModel;

         _windowRect = new Rect( 20, 20, _viewModel.Width, WindowHeight );

         _originalText = new ScrollPositioned();
         _defaultTranslation = new ScrollPositioned();
         _translationViews = viewModel.AvailableTranslators.Select( x => new ScrollPositioned<TranslatorViewModel>( x ) ).ToArray();
      }

      public bool IsShown
      {
         get => _viewModel.IsShown;
         set => _viewModel.IsShown = value;
      }

      private float WindowHeight => ( ( _viewModel.AvailableTranslators.Count( x => x.IsEnabled ) + 2 ) * _viewModel.Height ) + 30 + GUIUtil.LabelHeight + GUIUtil.ComponentSpacing;

      public void OnGUI()
      {
         _windowRect.height = WindowHeight;
         _windowRect.width = _viewModel.Width;
         _windowRect = GUI.Window( WindowId, _windowRect, (GUI.WindowFunction)CreateWindowUI, "---- Translation Aggregator ----" );

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

      public void Update()
      {
         _viewModel.Update();
      }

      public void OnNewTranslationAdded( string originalText, string defaultTranslation )
      {
         _viewModel.OnNewTranslationAdded( originalText, defaultTranslation );
      }

      private void CreateWindowUI( int id )
      {
         try
         {
            AutoTranslationPlugin.Current.DisableAutoTranslator();

            float posy = GUIUtil.WindowTitleClearance + GUIUtil.ComponentSpacing;

            if( GUI.Button( GUIUtil.R( _viewModel.Width - 22, 2, 20, 16 ), "X" ) )
            {
               IsShown = false;
            }

            var current = _viewModel.Current;
            if( current != null )
            {
               if( GUI.Button( GUIUtil.R( _viewModel.Width - GUIUtil.HalfComponentSpacing - 50, posy + 5 + 1, 50, GUIUtil.LabelHeight ), "Copy" ) )
               {
                  current.CopyOriginalTextToClipboard();
               }
               DrawTextArea( posy, _originalText, "Original Text", current.OriginalTexts );
               posy += _viewModel.Height;

               if( GUI.Button( GUIUtil.R( _viewModel.Width - GUIUtil.HalfComponentSpacing - 50, posy + 5 + 1, 50, GUIUtil.LabelHeight ), "Copy" ) )
               {
                  current.CopyDefaultTranslationToClipboard();
               }
               DrawTextArea( posy, _defaultTranslation, "Default Translation", current.DefaultTranslations );
               posy += _viewModel.Height;

               for( int i = 0; i < current.AggregatedTranslations.Count; i++ )
               {
                  var aggregatedTranslation = current.AggregatedTranslations[ i ];
                  if( aggregatedTranslation.Translator.IsEnabled )
                  {
                     var scroller = _translationViews[ i ];

                     GUI.enabled = aggregatedTranslation.CanCopyToClipboard();
                     if( GUI.Button( GUIUtil.R( _viewModel.Width - GUIUtil.HalfComponentSpacing - 50, posy + 5 + 1, 50, GUIUtil.LabelHeight ), "Copy" ) )
                     {
                        aggregatedTranslation.CopyToClipboard();
                     }
                     GUI.enabled = true;
                     DrawTextArea(
                        posy,
                        scroller,
                        aggregatedTranslation.Translator.Endpoint.Endpoint.FriendlyName,
                        aggregatedTranslation.Translation.Translations );
                     posy += _viewModel.Height;
                  }
               }
            }
            else
            {
               GUI.enabled = false;
               GUI.Button( GUIUtil.R( _viewModel.Width - GUIUtil.HalfComponentSpacing - 50, posy + 5 + 1, 50, GUIUtil.LabelHeight ), "Copy" );
               GUI.enabled = true;
               DrawTextArea( posy, _originalText, "Original Text", Empty );
               posy += _viewModel.Height;

               GUI.enabled = false;
               GUI.Button( GUIUtil.R( _viewModel.Width - GUIUtil.HalfComponentSpacing - 50, posy + 5 + 1, 50, GUIUtil.LabelHeight ), "Copy" );
               GUI.enabled = true;
               DrawTextArea( posy, _defaultTranslation, "Default Translation", Empty );
               posy += _viewModel.Height;

               for( int i = 0; i < _viewModel.AvailableTranslators.Count; i++ )
               {
                  var translator = _viewModel.AvailableTranslators[ i ];
                  if( translator.IsEnabled )
                  {
                     var scroller = _translationViews[ i ];

                     GUI.enabled = false;
                     GUI.Button( GUIUtil.R( _viewModel.Width - GUIUtil.HalfComponentSpacing - 50, posy + 5 + 1, 50, GUIUtil.LabelHeight ), "Copy" );
                     GUI.enabled = true;
                     DrawTextArea(
                        posy,
                        scroller,
                        translator.Endpoint.Endpoint.FriendlyName,
                        Empty );
                     posy += _viewModel.Height;
                  }
               }
            }

            posy += GUIUtil.HalfComponentSpacing + GUIUtil.ComponentSpacing;

            var previousEnabled = GUI.enabled;

            GUI.enabled = _viewModel.HasPrevious();
            if( GUI.Button( GUIUtil.R( GUIUtil.HalfComponentSpacing, posy, 75, GUIUtil.LabelHeight ), "Previous" ) )
            {
               _viewModel.MovePrevious();
            }

            GUI.enabled = _viewModel.HasNext();
            if( GUI.Button( GUIUtil.R( GUIUtil.HalfComponentSpacing * 2 + 75 * 1, posy, 75, GUIUtil.LabelHeight ), "Next" ) )
            {
               _viewModel.MoveNext();
            }

            GUI.enabled = _viewModel.HasNext();
            if( GUI.Button( GUIUtil.R( GUIUtil.HalfComponentSpacing * 3 + 75 * 2, posy, 75, GUIUtil.LabelHeight ), "Last" ) )
            {
               _viewModel.MoveLatest();
            }

            GUI.enabled = true;
            if( GUI.Button( GUIUtil.R( _viewModel.Width - GUIUtil.HalfComponentSpacing - 75, posy, 75, GUIUtil.LabelHeight ), "Options" ) )
            {
               _viewModel.IsShowingOptions = true;
            }

            GUI.enabled = previousEnabled;

            GUI.DragWindow();
         }
         finally
         {
            AutoTranslationPlugin.Current.EnableAutoTranslator();
         }
      }

      private void DrawTextArea( float posy, ScrollPositioned positioned, string title, IEnumerable<string> texts )
      {
         GUI.Label( GUIUtil.R( GUIUtil.HalfComponentSpacing + 5, posy + 5, _viewModel.Width - GUIUtil.ComponentSpacing, GUIUtil.LabelHeight ), title );

         posy += GUIUtil.LabelHeight + GUIUtil.HalfComponentSpacing;

         float boxWidth = _viewModel.Width - GUIUtil.ComponentSpacing;
         float boxHeight = _viewModel.Height - GUIUtil.LabelHeight;
         GUILayout.BeginArea( GUIUtil.R( GUIUtil.HalfComponentSpacing, posy, boxWidth, boxHeight ) );
         positioned.ScrollPosition = GUILayout.BeginScrollView( positioned.ScrollPosition, GUI.skin.box );

         foreach( var text in texts )
         {
            GUILayout.Label( text, GUIUtil.LabelTranslation, ArrayHelper.Null<GUILayoutOption>() );
         }

         GUILayout.EndScrollView();
         GUILayout.EndArea();
      }
   }
}
