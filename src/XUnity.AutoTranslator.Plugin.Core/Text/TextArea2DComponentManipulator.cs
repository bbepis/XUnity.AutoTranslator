using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   internal class TextArea2DComponentManipulator : ITextComponentManipulator
   {
      private readonly Action<object, object> set_status;
      private readonly Action<object, object> set_textData;
      private readonly Action<object, object> set_nameText;
      private readonly Action<object, bool> set_isInputSendMessage;
      private readonly CachedProperty _text;
      private readonly CachedProperty _TextData;

      public TextArea2DComponentManipulator()
      {
         var textDataField = ClrTypes.AdvPage.GetField( "textData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
         set_textData = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>( textDataField );

         var statusField = ClrTypes.AdvPage.GetField( "status", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
         set_status = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>( statusField );

         var isInputSendMessageField = ClrTypes.AdvPage.GetField( "isInputSendMessage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
         set_isInputSendMessage = CustomFastReflectionHelper.CreateFastFieldSetter<object, bool>( isInputSendMessageField );

         var nameTextField = ClrTypes.AdvPage.GetField( "nameText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
         set_nameText = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>( nameTextField );

         _text = ClrTypes.TextArea2D.CachedProperty( "text" );
         _TextData = ClrTypes.TextArea2D.CachedProperty( "TextData" );
      }

      public string GetText( object ui )
      {
         var textData = _TextData.Get( ui );
         if( textData != null )
         {
            return textData.GetExtensionData<string>();
         }
         return (string)_text.Get( ui );
      }

      public void SetText( object ui, string text )
      {
         if( ClrTypes.AdvUiMessageWindow != null && ClrTypes.AdvPage != null )
         {
            var uiMessageWindow = GameObject.FindObjectOfType( ClrTypes.AdvUiMessageWindow );

            var textUI = ClrTypes.AdvUiMessageWindow_Fields.text.Get( uiMessageWindow );
            var nameTextUI = ClrTypes.AdvUiMessageWindow_Fields.nameText.Get( uiMessageWindow );

            if( Equals( textUI, ui ) )
            {
               var advPage = GameObject.FindObjectOfType( ClrTypes.AdvPage );
               var textData = Activator.CreateInstance( ClrTypes.TextData, new object[] { text } );

               _TextData.Set( ui, textData ); // is this needed?
               set_textData( advPage, textData );
               set_status( advPage, 0 );
               set_isInputSendMessage( advPage, false );

               return;
            }
            else if( Equals( nameTextUI, ui ) )
            {
               var advPage = GameObject.FindObjectOfType( ClrTypes.AdvPage );
               var textData = Activator.CreateInstance( ClrTypes.TextData, new object[] { text } );

               _TextData.Set( ui, textData ); // is this needed?
               set_nameText( advPage, text );

               return;
            }
         }

         var textData2 = Activator.CreateInstance( ClrTypes.TextData, new object[] { text } );
         _text.Set( ui, text );
         _TextData.Set( ui, textData2 );
      }
   }

}
