using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text
{


   internal class DefaultTextComponentManipulator : ITextComponentManipulator
   {
      private static readonly string TextPropertyName = "text";

      private readonly Type _type;
      private readonly CachedProperty _property;

      public DefaultTextComponentManipulator( Type type )
      {
         _type = type;
         _property = type.CachedProperty( TextPropertyName );
      }

      public string GetText( object ui )
      {
         return (string)_property?.Get( ui );
      }

      public void SetText( object ui, string text )
      {
         var type = _type;

         if( ClrTypes.AdvUguiMessageWindow != null && ClrTypes.UguiNovelText?.IsAssignableFrom( type ) == true )
         {
            var uguiMessageWindow = GameObject.FindObjectOfType( ClrTypes.AdvUguiMessageWindow );
            if( uguiMessageWindow != null )
            {

               var uguiNovelText = ClrTypes.AdvUguiMessageWindow_Properties.Text?.Get( uguiMessageWindow )
                  ?? ClrTypes.AdvUguiMessageWindow_Fields.text?.GetValue( uguiMessageWindow );


               if( Equals( uguiNovelText, ui ) )
               {
                  string previousNameText = null;
                  var nameText = ClrTypes.AdvUguiMessageWindow_Fields.nameText.GetValue( uguiMessageWindow ) as UnityEngine.Object;
                  if( nameText )
                  {
                     previousNameText = (string)ClrTypes.Text.CachedProperty( TextPropertyName ).Get( nameText );
                  }

                  var engine = ClrTypes.AdvUguiMessageWindow_Properties.Engine?.Get( uguiMessageWindow )
                     ?? ClrTypes.AdvUguiMessageWindow_Fields.engine.GetValue( uguiMessageWindow );
                  var page = ClrTypes.AdvEngine_Properties.Page.Get( engine );

                  var remakeTextData = ClrTypes.AdvPage_Methods.RemakeTextData;
                  var remakeText = ClrTypes.AdvPage_Methods.RemakeText;
                  var changeMessageWindowText = ClrTypes.AdvPage_Methods.ChangeMessageWindowText;

                  if( changeMessageWindowText != null )
                  {
                     var nameText0 = (string)page.GetType().GetProperty( "NameText" )?.GetValue( page, null );
                     var characterLabel0 = (string)page.GetType().GetProperty( "CharacterLabel" )?.GetValue( page, null );
                     var windowType0 = (string)page.GetType().GetProperty( "WindowType" )?.GetValue( page, null );

                     changeMessageWindowText.Invoke( page, new object[] { nameText0, characterLabel0, text, windowType0 } );
                  }
                  else
                  {
                     var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                     var textData = ClrTypes.TextData.GetConstructor( new[] { typeof( string ) } ).Invoke( new[] { text } );
                     var length = (int)textData.GetType().GetProperty( "Length", flags ).GetValue( textData, null );

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

                           remakeText.Invoke( page );
                        }
                        finally
                        {
                           Settings.InvokeEvents = true;
                           Settings.RemakeTextData = null;
                        }
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

         // fallback to reflective approach
         var property = _property;
         if( property != null )
         {
            property.Set( ui, text );

            if( Settings.IgnoreVirtualTextSetterCallingRules )
            {
               var newText = (string)property.Get( ui );
               Type currentType = type;
               while( text != newText && currentType != null )
               {
                  var typeAndMethod = GetTextPropertySetterInParent( currentType );
                  if( typeAndMethod != null )
                  {
                     currentType = typeAndMethod.Type;

                     typeAndMethod.SetterInvoker( ui, text );
                     newText = (string)property.Get( ui );
                  }
                  else
                  {
                     currentType = null;
                  }
               }
            }
         }

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

         if( ClrTypes.TextExpansion_Methods.SetMessageType != null && ClrTypes.TextExpansion_Methods.SkipTypeWriter != null )
         {
            if( ClrTypes.TextExpansion.IsAssignableFrom( type ) )
            {
               ClrTypes.TextExpansion_Methods.SetMessageType.Invoke( ui, 1 );
               ClrTypes.TextExpansion_Methods.SkipTypeWriter.Invoke( ui );
            }
         }
      }

      private static Dictionary<Type, TypeAndMethod> _textSetters = new Dictionary<Type, TypeAndMethod>();
      private static TypeAndMethod GetTextPropertySetterInParent( Type type )
      {
         var current = type.BaseType;
         while( current != null )
         {
            if( _textSetters.TryGetValue( type, out var typeAndMethod ) )
            {
               return typeAndMethod;
            }
            else
            {
               var property = current.GetProperty( TextPropertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

               if( property != null && property.CanWrite )
               {
                  var tam = new TypeAndMethod( current, property.GetSetMethod() );
                  _textSetters[ current ] = tam;
                  return tam;
               }

               current = current.BaseType;
            }
         }

         return null;
      }

      private class TypeAndMethod
      {
         FastReflectionDelegate _setterInvoker;

         public TypeAndMethod( Type type, MethodBase method )
         {
            Type = type;
            SetterMethod = method;
         }

         public Type Type { get; }
         public MethodBase SetterMethod { get; }
         public FastReflectionDelegate SetterInvoker
         {
            get
            {
               return _setterInvoker ?? ( _setterInvoker = CustomFastReflectionHelper.CreateFastDelegate( SetterMethod, true, true ) );
            }
         }
      }
   }

}
