using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class TextComponentExtensions
   {
      private static readonly string SupportRichTextPropertyName = "supportRichText";
      private static readonly string RichTextPropertyName = "richText";
      private static readonly string TextPropertyName = "text";

      //private static readonly GUIContent[] TemporaryGUIContents = ClrTypes.GUIContent
      //   .GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
      //   .Where( x => x.DeclaringType == typeof( GUIContent ) && ( x.Name == "s_Text" || x.Name == "s_TextImage" ) )
      //   .Select( x => (GUIContent)x.GetValue( null ) )
      //   .ToArray();

      public static bool IsKnownTextType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( Settings.EnableIMGUI && ui is GUIContent )
            || ( Settings.EnableUGUI && ClrTypes.Text != null && ClrTypes.Text.IsAssignableFrom( type ) )
            || ( Settings.EnableNGUI && ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMesh && ClrTypes.TextMesh != null && ClrTypes.TextMesh.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && IsKnownTextMeshProType( type ) )
            /*|| ( ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type ) )*/;
      }

      public static bool IsKnownTextMeshProType( Type type )
      {
         if( ClrTypes.TMP_Text != null )
         {
            return ClrTypes.TMP_Text.IsAssignableFrom( type );
         }
         else
         {
            return ClrTypes.TextMeshProUGUI?.IsAssignableFrom( type ) == true
            || ClrTypes.TextMeshPro?.IsAssignableFrom( type ) == true;
         }
      }

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( ClrTypes.Text != null && ClrTypes.Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( SupportRichTextPropertyName )?.Get( ui ), true ) )
            || ( ClrTypes.TextMesh != null && ClrTypes.TextMesh.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
            || DoesTextMeshProSupportRichText( ui, type )
            || ( ClrTypes.UguiNovelText != null && ClrTypes.UguiNovelText.IsAssignableFrom( type ) )
            /*|| ( ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type ) )*/;
      }

      public static bool DoesTextMeshProSupportRichText( object ui, Type type )
      {
         if( ClrTypes.TMP_Text != null )
         {
            return ClrTypes.TMP_Text.IsAssignableFrom( type ) && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true );
         }
         else 
         {
            return ( ClrTypes.TextMeshPro?.IsAssignableFrom( type ) == true && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) )
               || ( ClrTypes.TextMeshProUGUI?.IsAssignableFrom( type ) == true && Equals( type.CachedProperty( RichTextPropertyName )?.Get( ui ), true ) );
         }
      }

      public static bool SupportsStabilization( this object ui )
      {
         if( ui == null ) return false;

         // black listing of components not supporting stabilization
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
         
         return true;
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

         TextGetterCompatModeHelper.IsGettingText = true;
         try
         {
            if( ui is GUIContent )
            {
               text = ( (GUIContent)ui ).text;
            }
            else
            {
               // fallback to reflective approach
               text = (string)type.CachedProperty( TextPropertyName )?.Get( ui );
            }
         }
         finally
         {
            TextGetterCompatModeHelper.IsGettingText = false;
         }

         return text ?? string.Empty;
      }

      public static void SetText( this object ui, string text )
      {
         if( ui == null ) return;

         var type = ui.GetType();

         if( ClrTypes.AdvUguiMessageWindow != null && ClrTypes.UguiNovelText?.IsAssignableFrom( type ) == true )
         {
            var uguiMessageWindow = GameObject.FindObjectOfType( ClrTypes.AdvUguiMessageWindow );
            if( uguiMessageWindow != null )
            {
               var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

               var uguiNovelText = uguiMessageWindow.GetType().GetProperty( "Text" ).GetValue( uguiMessageWindow, null );
               if( Equals( uguiNovelText, ui ) )
               {
                  string previousNameText = null;
                  var nameText = (UnityEngine.Object)uguiMessageWindow.GetType().GetField( "nameText", flags ).GetValue( uguiMessageWindow );
                  if( nameText )
                  {
                     previousNameText = (string)ClrTypes.Text.CachedProperty( TextPropertyName ).Get( nameText );
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
                     ClrTypes.Text.CachedProperty( TextPropertyName ).Set( nameText, previousNameText );
                  }

                  return;
               }
            }
         }

         if( ClrTypes.TextWindow != null && ClrTypes.TextMeshPro?.IsAssignableFrom( type ) == true )
         {
            var textWindow = GameObject.FindObjectOfType( ClrTypes.TextWindow );
            if( textWindow != null )
            {
               var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
               var textMesh = textWindow.GetType().GetField( "TextMesh", flags )?.GetValue( textWindow );
               if( textMesh != null )
               {
                  if( Equals( textMesh, ui ) )
                  {
                     var frames = new StackTrace().GetFrames();
                     if( frames.Any( x => x.GetMethod().DeclaringType == ClrTypes.TextWindow ) )
                     {
                        // If inline (sync)

                        var previousCurText = textWindow.GetType().GetField( "curText", flags ).GetValue( textWindow );
                        textWindow.GetType().GetField( "curText", flags ).SetValue( textWindow, text );

                        Settings.SetCurText = textWindowInner =>
                        {
                           var translatedText = textWindow.GetType().GetField( "curText", flags ).GetValue( textWindow );
                           if( Equals( text, translatedText ) )
                           {
                              textWindowInner.GetType().GetMethod( "FinishTyping", flags ).Invoke( textWindowInner, null );

                              textWindowInner.GetType().GetField( "curText", flags ).SetValue( textWindowInner, previousCurText );

                              var textMeshInner = textWindowInner.GetType().GetField( "TextMesh", flags ).GetValue( textWindowInner );
                              var keyword = textWindowInner.GetType().GetField( "Keyword", flags ).GetValue( textWindowInner );
                              keyword.GetType().GetMethod( "UpdateTextMesh", flags ).Invoke( keyword, new object[] { textMeshInner, true } );
                           }

                           Settings.SetCurText = null;
                        };
                     }
                     else
                     {
                        // If delayed by plugin (async)

                        // update the actual text in the text box
                        type.CachedProperty( TextPropertyName )?.Set( ui, text );

                        // obtain the curText in the text window
                        var previousCurText = textWindow.GetType().GetField( "curText", flags ).GetValue( textWindow );

                        // set curText to OUR text
                        textWindow.GetType().GetField( "curText", flags ).SetValue( textWindow, text );

                        // call FinishTyping
                        textWindow.GetType().GetMethod( "FinishTyping", flags ).Invoke( textWindow, null );

                        // reset curText
                        textWindow.GetType().GetField( "curText", flags ).SetValue( textWindow, previousCurText );

                        var keyword = textWindow.GetType().GetField( "Keyword", flags ).GetValue( textWindow );
                        keyword.GetType().GetMethod( "UpdateTextMesh", flags ).Invoke( keyword, new object[] { textMesh, true } );
                     }

                     return;
                  }
               }
            }
         }


         if( ui is GUIContent )
         {
            ( (GUIContent)ui ).text = text;
         }
         else
         {
            // fallback to reflective approach
            type.CachedProperty( TextPropertyName )?.Set( ui, text );

            // TMPro
            var maxVisibleCharactersProperty = type.CachedProperty( "maxVisibleCharacters" );
            if( maxVisibleCharactersProperty != null && maxVisibleCharactersProperty.PropertyType == typeof( int ) )
            {
               var value = (int)maxVisibleCharactersProperty.Get( ui );
               if( 0 < value && value < 99999 )
               {
                  maxVisibleCharactersProperty.Set( ui, 99999 );
               }
            }
         }
      }
   }
}
