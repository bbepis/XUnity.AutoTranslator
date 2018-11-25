using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using UnityEngine.UI;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class ObjectExtensions
   {
      private static readonly string RichTextPropertyName = "richText";

      private static readonly object Sync = new object();
      private static readonly WeakDictionary<object, object> DynamicFields = new WeakDictionary<object, object>();

      public static bool IsKnownTextType( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( Settings.EnableUGUI && ui is Text )
            || ( Settings.EnableIMGUI && ui is GUIContent )
            || ( Settings.EnableNGUI && ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type ) )
            || ( Settings.EnableTextMeshPro && ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) )
            || ( Settings.EnableUtage && ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type ) );
      }

      public static bool IsKnownImageType( this object ui )
      {
         var type = ui.GetType();

         // Simplify this??

         return ( Settings.EnableUGUI && ( ui is Material || ui is Image || ui is RawImage ) )
            || ( Settings.EnableNGUI
               && ( ( ClrTypes.UIWidget != null && type != ClrTypes.UILabel && ClrTypes.UIWidget.IsAssignableFrom( type ) )
               || ( ClrTypes.UIAtlas != null && ClrTypes.UIAtlas.IsAssignableFrom( type ) )
               || ( ClrTypes.UITexture != null && ClrTypes.UITexture.IsAssignableFrom( type ) )
               //|| ( ClrTypes.UIFont != null && ClrTypes.UIFont.IsAssignableFrom( type ) )
               || ( ClrTypes.UIPanel != null && ClrTypes.UIPanel.IsAssignableFrom( type ) ) )
            );
      }

      public static bool SupportsStabilization( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ui is Text
            || ( ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type ) )
            || ( ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) );
      }

      public static bool SupportsRichText( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ( ui as Text )?.supportRichText == true
            || ( ClrTypes.TMP_Text != null && ClrTypes.TMP_Text.IsAssignableFrom( type ) && Equals( type.GetProperty( RichTextPropertyName )?.GetValue( ui, null ), true ) )
            || ( ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type ) )
            || ( ClrTypes.UguiNovelText != null && ClrTypes.UguiNovelText.IsAssignableFrom( type ) );
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

         return ClrTypes.AdvCommand != null && ClrTypes.AdvCommand.IsAssignableFrom( type );
      }

      public static bool IsNGUI( this object ui )
      {
         if( ui == null ) return false;

         var type = ui.GetType();

         return ClrTypes.UILabel != null && ClrTypes.UILabel.IsAssignableFrom( type );
      }

      public static TextTranslationInfo GetTextTranslationInfo( this object obj )
      {
         if( !Settings.EnableObjectTracking ) return null;

         if( !obj.SupportsStabilization() ) return null;

         var info = obj.Get<TextTranslationInfo>();

         return info;
      }

      public static ImageTranslationInfo GetImageTranslationInfo( this object obj )
      {
         return obj.Get<ImageTranslationInfo>();
      }

      public static TextureTranslationInfo GetTextureTranslationInfo( this Texture texture )
      {
         return texture.Get<TextureTranslationInfo>();
      }

      public static T Get<T>( this object obj )
         where T : new()
      {
         if( obj == null ) return default( T );

         lock( Sync )
         {
            if( DynamicFields.TryGetValue( obj, out object value ) )
            {
               if( value is Dictionary<Type, object> existingDictionary )
               {
                  if( existingDictionary.TryGetValue( typeof( T ), out value ) )
                  {
                     return (T)value;
                  }
                  else
                  {
                     var t = new T();
                     existingDictionary[ typeof( T ) ] = t;
                     return t;
                  }
               }
               if( !( value is T ) )
               {
                  var newDictionary = new Dictionary<Type, object>();
                  newDictionary.Add( value.GetType(), value );
                  var t = new T();
                  newDictionary[ typeof( T ) ] = t;
                  DynamicFields[ obj ] = newDictionary;
                  return t;
               }
               else
               {
                  return (T)value;
               }
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
            return IterateAllPairs().ToList();
         }
      }

      public static void Remove( object obj )
      {
         lock( Sync )
         {
            DynamicFields.Remove( obj );
         }
      }

      private static IEnumerable<KeyValuePair<object, object>> IterateAllPairs()
      {
         foreach( var kvp in DynamicFields )
         {
            if( kvp.Value is Dictionary<Type, object> dictionary )
            {
               foreach( var kvp2 in dictionary )
               {
                  yield return new KeyValuePair<object, object>( kvp.Key, kvp2.Value );
               }
            }
            else
            {
               yield return kvp;
            }
         }
      }
   }
}
