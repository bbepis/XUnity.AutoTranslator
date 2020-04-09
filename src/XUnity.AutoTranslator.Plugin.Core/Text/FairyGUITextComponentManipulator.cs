using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   internal class FairyGUITextComponentManipulator : ITextComponentManipulator
   {
      private readonly CachedField _html;
      private readonly CachedProperty _htmlText;
      private readonly CachedProperty _text;

      public FairyGUITextComponentManipulator()
      {
         _html = ClrTypes.TextField.CachedField( "html" ) ?? ClrTypes.TextField.CachedFieldByIndex( 3, typeof( bool ), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
         _text = ClrTypes.TextField.CachedProperty( "text" );
         _htmlText = ClrTypes.TextField.CachedProperty( "htmlText" );
      }

      public string GetText( object ui )
      {
         var html = (bool)_html.Get( ui );
         if( html )
         {
            return (string)_htmlText.Get( ui );
         }
         else
         {
            return (string)_text.Get( ui );
         }
      }

      public void SetText( object ui, string text )
      {
         var html = (bool)_html.Get( ui );
         if( html )
         {
            _htmlText.Set( ui, text );
         }
         else
         {
            _text.Set( ui, text );
         }
      }
   }
}
