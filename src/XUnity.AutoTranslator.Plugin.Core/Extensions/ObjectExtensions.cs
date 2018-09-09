using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class ObjectExtensions
   {
      private static readonly string RichTextPropertyName = "richText";

      private static readonly object Sync = new object();
      private static readonly WeakDictionary<object, object> DynamicFields = new WeakDictionary<object, object>();

      public static bool IsKnownType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ui is Text
            || ui is UnityEngine.GUIContent
            || ( Types.UILabel != null && Types.UILabel.IsAssignableFrom( type ) )
            || ( Types.TMP_Text != null && Types.TMP_Text.IsAssignableFrom( type ) )
            || ( Types.AdvCommand != null && Types.AdvCommand.IsAssignableFrom( type ) );
      }

      public static bool SupportsStabilization( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ui is Text
            || ( Types.UILabel != null && Types.UILabel.IsAssignableFrom( type ) )
            || ( Types.TMP_Text != null && Types.TMP_Text.IsAssignableFrom( type ) );
      }

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( ui as Text )?.supportRichText == true
            || ( Types.TMP_Text != null && Types.TMP_Text.IsAssignableFrom( type ) && Equals( type.GetProperty( RichTextPropertyName )?.GetValue( ui, null ), true ) )
            || ( Types.AdvCommand != null && Types.AdvCommand.IsAssignableFrom( type ) )
            || ( Types.UguiNovelText != null && Types.UguiNovelText.IsAssignableFrom( type ) );
      }

      public static bool IsSpammingComponent( this object ui )
      {
         if( ui == null ) return false;

         return ui is UnityEngine.GUIContent;
      }

      public static bool IsWhitelistedForImmediateRichTextTranslation( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return Types.AdvCommand != null && Types.AdvCommand.IsAssignableFrom( type );
      }

      public static bool IsNGUI( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return Types.UILabel != null && Types.UILabel.IsAssignableFrom( type );
      }

      public static TranslationInfo GetTranslationInfo( this object obj )
      {
         if( !Settings.EnableObjectTracking ) return null;

         if( !obj.SupportsStabilization() ) return null;

         var info = obj.Get<TranslationInfo>();

         return info;
      }

      public static T Get<T>( this object obj )
         where T : new()
      {
         lock( Sync )
         {
            if( DynamicFields.TryGetValue( obj, out object value ) )
            {
               return (T)value;
            }
            else
            {
               var t = new T();
               DynamicFields[ obj ] = t;
               return t;
            }
         }
      }

      public static void Cull()
      {
         lock( Sync )
         {
            DynamicFields.RemoveCollectedEntries();
         }
      }

      public static List<KeyValuePair<object, object>> GetAllRegisteredObjects()
      {
         lock( Sync )
         {
            return DynamicFields.ToList();
         }
      }

      public static void Remove( object obj )
      {
         lock( Sync )
         {
            DynamicFields.Remove( obj );
         }
      }
   }
}
