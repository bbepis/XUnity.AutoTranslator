using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextComponentExtensions
   {
      //private static readonly GUIContent[] TemporaryGUIContents = ClrTypes.GUIContent
      //   .GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
      //   .Where( x => x.DeclaringType == typeof( GUIContent ) && ( x.Name == "s_Text" || x.Name == "s_TextImage" ) )
      //   .Select( x => (GUIContent)x.GetValue( null ) )
      //   .ToArray();

      private static readonly string RichTextPropertyName = "richText";
      private static readonly string TextPropertyName = "text";

      public static bool IsKnownTextType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( Settings.EnableUGUI && ui is Text )
            || ( Settings.EnableIMGUI && ui is GUIContent )
            || ( Settings.EnableNGUI && ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) )
            /*|| ( ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type ) )*/;
      }

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( ui as Text )?.supportRichText == true
            || ( ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) && Equals( type.GetProperty( RichTextPropertyName )?.GetValue( ui, null ), true ) )
            || ( ClrTypes.UguiNovelText != null && ClrTypes.UguiNovelText.IsAssignableFrom( type ) )
            /*|| ( ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type ) )*/;
      }

      public static bool SupportsStabilization( this object ui )
      {
         if( ui == null ) return false;

         // shortcircuit for spammy component, to avoid reflective calls
         if( ui is GUIContent )
         {
            return false;

            //var len = TemporaryGUIContents.Length;
            //for( int i = 0; i < len; i++ )
            //{
            //   if( ReferenceEquals( ui, TemporaryGUIContents[ i ] ) )
            //   {
            //      return false;
            //   }
            //}
            //return true;
         }

         var type = ui.GetType();

         return ui is Text
            || ( ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type ) )
            || ( ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) );
      }

      public static bool SupportsLineParser( this object ui )
      {
         return Settings.GameLogTextPaths.Count > 0 && ui is Component comp && Settings.GameLogTextPaths.Contains( comp.gameObject.GetPath() );
      }

      public static bool IsSpammingComponent( this object ui )
      {
         if( ui == null ) return true;

         return ui is GUIContent;
      }

      //public static bool IsWhitelistedForImmediateRichTextTranslation( this object ui )
      //{
      //   if( ui == null ) return false;

      //   var type = ui.GetType();

      //   return ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type );
      //}

      public static bool IsNGUI( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type );
      }

      public static string GetText( this object ui )
      {
         if( ui == null ) return null;

         string text = null;
         var type = ui.GetType();

         if( ui is Text )
         {
            text = ( (Text)ui ).text;
         }
         else if( ui is GUIContent )
         {
            text = ( (GUIContent)ui ).text;
         }
         else
         {
            // fallback to reflective approach
            text = (string)ui.GetType()?.GetProperty( TextPropertyName )?.GetValue( ui, null );
         }

         return text ?? string.Empty;
      }

      public static void SetText( this object ui, string text )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( ClrTypes.UguiNovelText?.IsAssignableFrom( type ) == true )
         {
            var uguiMessageWindow = GameObject.FindObjectOfType( ClrTypes.AdvUguiMessageWindow );
            if( uguiMessageWindow != null )
            {
               var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

               var uguiNovelText = uguiMessageWindow.GetType().GetProperty( "Text" ).GetValue( uguiMessageWindow, null );
               if( Equals( uguiNovelText, ui ) )
               {
                  string previousNameText = null;
                  var nameText = (Text)uguiMessageWindow.GetType().GetField( "nameText", flags ).GetValue( uguiMessageWindow );
                  if( nameText )
                  {
                     previousNameText = nameText.text;
                  }

                  var engine = uguiMessageWindow.GetType().GetProperty( "Engine", flags ).GetValue( uguiMessageWindow, null );
                  var page = engine.GetType().GetProperty( "Page", flags ).GetValue( engine, null );
                  var textData = ClrTypes.TextData.GetConstructor( new[] { typeof( string ) } ).Invoke( new[] { text } );
                  var length = (int)textData.GetType().GetProperty( "Length", flags ).GetValue( textData, null );

                  var remakeTextData = page.GetType().GetMethod( "RemakeTextData", flags );
                  if( remakeTextData == null )
                  {
                     try
                     {
                        Settings.InvokeEvents = false;

                        page.GetType().GetProperty( "TextData", flags ).SetValue( page, textData, null );
                        page.GetType().GetProperty( "CurrentTextLengthMax", flags ).GetSetMethod( true ).Invoke( page, new object[] { length } );
                        page.GetType().GetProperty( "Status", flags ).GetSetMethod( true ).Invoke( page, new object[] { 1 /*SendChar*/ } );

                        var messageWindowManager = engine.GetType().GetProperty( "MessageWindowManager", flags ).GetValue( engine, null );
                        messageWindowManager.GetType().GetMethod( "OnPageTextChange", flags ).Invoke( messageWindowManager, new object[] { page } );
                     }
                     finally
                     {
                        Settings.InvokeEvents = true;
                     }
                  }
                  else
                  {
                     try
                     {
                        Settings.InvokeEvents = false;
                        Settings.RemakeTextData = advPage =>
                        {
                           advPage.GetType().GetProperty( "TextData", flags ).SetValue( page, textData, null );
                           advPage.GetType().GetProperty( "CurrentTextLengthMax", flags ).GetSetMethod( true ).Invoke( page, new object[] { length } );
                        };

                        page.GetType().GetMethod( "RemakeText" ).Invoke( page, null );
                     }
                     finally
                     {
                        Settings.InvokeEvents = true;
                        Settings.RemakeTextData = null;
                     }
                  }

                  if( nameText )
                  {
                     nameText.text = previousNameText;
                  }

                  return;
               }
            }
         }

         if( ui is Text )
         {
            ( (Text)ui ).text = text;
         }
         else if( ui is GUIContent )
         {
            ( (GUIContent)ui ).text = text;
         }
         else
         {
            // fallback to reflective approach
            type.GetProperty( TextPropertyName )?.GetSetMethod()?.Invoke( ui, new[] { text } );
         }
      }
   }
}
