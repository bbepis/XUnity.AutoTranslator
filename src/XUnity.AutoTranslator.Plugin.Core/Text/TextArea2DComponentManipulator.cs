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
         var textDataField = UnityTypes.AdvPage.ClrType.GetField( "textData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
         set_textData = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>( textDataField );

         var statusField = UnityTypes.AdvPage.ClrType.GetField( "status", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
         set_status = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>( statusField );

         var isInputSendMessageField = UnityTypes.AdvPage.ClrType.GetField( "isInputSendMessage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
         set_isInputSendMessage = CustomFastReflectionHelper.CreateFastFieldSetter<object, bool>( isInputSendMessageField );

         var nameTextField = UnityTypes.AdvPage.ClrType.GetField( "nameText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
         set_nameText = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>( nameTextField );

         _text = UnityTypes.TextArea2D.ClrType.CachedProperty( "text" );
         _TextData = UnityTypes.TextArea2D.ClrType.CachedProperty( "TextData" );
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
         if( UnityTypes.AdvUiMessageWindow != null && UnityTypes.AdvPage != null )
         {
            var uiMessageWindow = GameObject.FindObjectOfType( UnityTypes.AdvUiMessageWindow.UnityType );

            var textUI = UnityTypes.AdvUiMessageWindow_Fields.text.Get( uiMessageWindow );
            var nameTextUI = UnityTypes.AdvUiMessageWindow_Fields.nameText.Get( uiMessageWindow );

            if( Equals( textUI, ui ) )
            {
               var advPage = GameObject.FindObjectOfType( UnityTypes.AdvPage.UnityType );
               var textData = Activator.CreateInstance( UnityTypes.TextData.ClrType, new object[] { text } );

               _TextData.Set( ui, textData ); // is this needed?
               set_textData( advPage, textData );
               set_status( advPage, 0 );
               set_isInputSendMessage( advPage, false );

               return;
            }
            else if( Equals( nameTextUI, ui ) )
            {
               var advPage = GameObject.FindObjectOfType( UnityTypes.AdvPage.UnityType );
               var textData = Activator.CreateInstance( UnityTypes.TextData.ClrType, new object[] { text } );

               _TextData.Set( ui, textData ); // is this needed?
               set_nameText( advPage, text );

               return;
            }
         }

         var textData2 = Activator.CreateInstance( UnityTypes.TextData.ClrType, new object[] { text } );
         _text.Set( ui, text );
         _TextData.Set( ui, textData2 );
      }
   }
}
