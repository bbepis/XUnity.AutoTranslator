using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal static class GUIUtil
   {
      public const float WindowTitleClearance = 10;
      public const float ComponentSpacing = 10;
      public const float HalfComponentSpacing = ComponentSpacing / 2;
      public const float LabelWidth = 60;
      public const float LabelHeight = 21;
      public const float RowHeight = 21;

      public static GUIContent none = new GUIContent( "" );

      public static readonly RectOffset Empty = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };

      public static readonly GUIStyle LabelTranslation = CopyStyle( GUI.skin.label, style =>
      {
         style.richText = false;
         style.margin = new RectOffset { left = GUI.skin.label.margin.left, right = GUI.skin.label.margin.right, top = 0, bottom = 0 };
         style.padding = new RectOffset { left = GUI.skin.label.padding.left, right = GUI.skin.label.padding.right, top = 2, bottom = 3 };
      } );

      public static readonly GUIStyle LabelCenter = CopyStyle( GUI.skin.label, style =>
      {
         style.alignment = TextAnchor.UpperCenter;
         style.richText = true;
      } );

      public static readonly GUIStyle LabelRight = CopyStyle( GUI.skin.label, style =>
      {
         style.alignment = TextAnchor.UpperRight;
      } );

      public static readonly GUIStyle LabelRich = CopyStyle( GUI.skin.label, style =>
      {
         style.richText = true;
      } );

      public static readonly GUIStyle NoMarginButtonStyle = CopyStyle( GUI.skin.button, style =>
      {
         style.margin = Empty;
      } );

      public static readonly GUIStyle NoMarginButtonPressedStyle = CopyStyle( GUI.skin.button, style =>
      {
         style.margin = Empty;
         style.onNormal = GUI.skin.button.onActive;
         style.onFocused = GUI.skin.button.onActive;
         style.onHover = GUI.skin.button.onActive;
         style.normal = GUI.skin.button.onActive;
         style.focused = GUI.skin.button.onActive;
         style.hover = GUI.skin.button.onActive;
      } );

      public static readonly GUIStyle NoSpacingBoxStyle = CopyStyle( GUI.skin.box, style =>
      {
         style.margin = Empty;
         style.padding = Empty;
      } );

      public static GUIStyle WindowBackgroundStyle = new GUIStyle
      {
         normal = new GUIStyleState
         {
            background = CreateBackgroundTexture()
         }
      };

      public static GUIStyle CopyStyle( GUIStyle other, Action<GUIStyle> setProperties )
      {
#if IL2CPP
         return CopyWithPropertiesSafe( other, setProperties );
#else
         var style = new GUIStyle( other );
         setProperties( style );
         return style;
#endif
      }

#if IL2CPP
      private static bool _hasCtor = true;
      
      public static GUIStyle CopyWithPropertiesSafe( GUIStyle other, Action<GUIStyle> setProperties )
      {
         if( _hasCtor )
         {
            try
            {
               return CopyWithPropertiesUnsafe( other, setProperties );
            }
            catch
            {
               _hasCtor = false;
            }
         }
         return CopyWithPropertiesSpecial( other, setProperties );
      }

      public static GUIStyle CopyWithPropertiesSpecial( GUIStyle other, Action<GUIStyle> setProperties )
      {
         var style = new GUIStyle();
         style.m_Ptr = GUIStyle.Internal_Copy( style, other );
         setProperties( style );
         return style;
      }

      public static GUIStyle CopyWithPropertiesUnsafe( GUIStyle other, Action<GUIStyle> setProperties )
      {
         var style = new GUIStyle( other );
         setProperties( style );
         return style;
      }
#endif

      public static GUIContent CreateContent( string text )
      {
         return new GUIContent( text );
      }

      public static GUIContent CreateContent( string text, string tooltip )
      {
#if IL2CPP
         return new GUIContent( text, null, tooltip );
#else
         return new GUIContent( text, tooltip );
#endif
      }

      public static Rect R( float x, float y, float width, float height ) => new Rect( x, y, width, height );

      private static Texture2D CreateBackgroundTexture()
      {
         var windowBackground = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
         windowBackground.SetPixel( 0, 0, new Color( 0.6f, 0.6f, 0.6f, 1 ) );
         windowBackground.Apply();
         GameObject.DontDestroyOnLoad( windowBackground );
         return windowBackground;
      }

      public static GUIStyle GetWindowBackgroundStyle()
      {
         if( !WindowBackgroundStyle.normal.background )
         {
            WindowBackgroundStyle = new GUIStyle
            {
               normal = new GUIStyleState
               {
                  background = CreateBackgroundTexture()
               }
            };
         }

         return WindowBackgroundStyle;
      }

      public static bool IsAnyMouseButtonOrScrollWheelDownSafe
      {
         get
         {
            return UnityFeatures.SupportsMouseScrollDelta
               ? IsAnyMouseButtonOrScrollWheelDown
               : IsAnyMouseButtonOrScrollWheelDownLegacy;
         }
      }

      public static bool IsAnyMouseButtonOrScrollWheelDownLegacy
      {
         get
         {
            return Input.GetMouseButtonDown( 0 )
               || Input.GetMouseButtonDown( 1 )
               || Input.GetMouseButtonDown( 2 );
         }
      }

      public static bool IsAnyMouseButtonOrScrollWheelDown
      {
         get
         {
            return Input.mouseScrollDelta.y != 0f
               || Input.GetMouseButtonDown( 0 )
               || Input.GetMouseButtonDown( 1 )
               || Input.GetMouseButtonDown( 2 );
         }
      }

      public static bool IsAnyMouseButtonOrScrollWheelSafe
      {
         get
         {
            return UnityFeatures.SupportsMouseScrollDelta
               ? IsAnyMouseButtonOrScrollWheel
               : IsAnyMouseButtonOrScrollWheelLegacy;
         }
      }

      public static bool IsAnyMouseButtonOrScrollWheelLegacy
      {
         get
         {
            return Input.GetMouseButton( 0 )
               || Input.GetMouseButton( 1 )
               || Input.GetMouseButton( 2 );
         }
      }

      public static bool IsAnyMouseButtonOrScrollWheel
      {
         get
         {
            return Input.mouseScrollDelta.y != 0f
               || Input.GetMouseButton( 0 )
               || Input.GetMouseButton( 1 )
               || Input.GetMouseButton( 2 );
         }
      }
   }
}
