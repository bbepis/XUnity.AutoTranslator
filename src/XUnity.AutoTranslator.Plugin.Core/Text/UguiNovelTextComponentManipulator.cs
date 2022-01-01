using System;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text
{
   internal class UguiNovelTextComponentManipulator : ITextComponentManipulator
   {
      private static readonly string TextPropertyName = "text";
      private readonly Type _type;
      private readonly CachedProperty _property;

      public UguiNovelTextComponentManipulator( Type type )
      {
         _type = type;
         _property = type.CachedProperty( TextPropertyName );
      }

      public string GetText( object ui )
      {
         return (string)_property.Get( ui );
      }

      public void SetText( object ui, string text )
      {
         // must also update TextData

         _property.Set( ui, text );

         UnityTypes.UguiNovelText_Methods.SetAllDirty.Invoke( ui );
         var textGenerator = UnityTypes.UguiNovelText_Properties.TextGenerator.Get( ui );
         UnityTypes.UguiNovelTextGenerator_Methods.Refresh.Invoke( textGenerator );
      }
   }

}
