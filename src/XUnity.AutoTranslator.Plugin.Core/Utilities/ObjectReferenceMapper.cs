using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class ObjectReferenceMapper
   {
      private static readonly object Sync = new object();
      private static readonly WeakDictionary<object, object> DynamicFields = new WeakDictionary<object, object>();

      public static TextTranslationInfo GetOrCreateTextTranslationInfo( this object ui )
      {
         if( ui.SupportsStabilization() && ui.IsKnownTextType() )
         {
            var info = ui.GetOrCreateExtensionData<TextTranslationInfo>();

            return info;
         }

         return null;
      }

      public static TextTranslationInfo GetTextTranslationInfo( this object ui )
      {
         if( ui.SupportsStabilization() && ui.IsKnownTextType() )
         {
            var info = ui.GetExtensionData<TextTranslationInfo>();

            return info;
         }

         return null;
      }

      public static ImageTranslationInfo GetOrCreateImageTranslationInfo( this object obj )
      {
         return obj.GetOrCreateExtensionData<ImageTranslationInfo>();
      }

      public static TextureTranslationInfo GetOrCreateTextureTranslationInfo( this Texture2D texture )
      {
         var tti = texture.GetOrCreateExtensionData<TextureTranslationInfo>();
         if( tti.Original == null ) tti.SetOriginal( texture );

         return tti;
      }

      public static void Set<T>( this object obj, T t )
         where T : new()
      {
         lock( Sync )
         {
            if( DynamicFields.TryGetValue( obj, out object value ) )
            {
               if( value is Dictionary<Type, object> existingDictionary )
               {
                  existingDictionary[ typeof( T ) ] = t;
               }
               else
               {
                  var newDictionary = new Dictionary<Type, object>();
                  newDictionary.Add( value.GetType(), value );
                  newDictionary[ typeof( T ) ] = t;
                  DynamicFields[ obj ] = newDictionary;
               }
            }
            else
            {
               DynamicFields[ obj ] = t;
            }
         }
      }

      public static T GetOrCreateExtensionData<T>( this object obj )
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

      public static T GetExtensionData<T>( this object obj )
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
                     if( value is T )
                     {
                        return (T)value;
                     }
                     else
                     {
                        return default( T );
                     }
                  }
               }

               if( value is T )
               {
                  return (T)value;
               }
               else
               {
                  return default( T );
               }
            }
         }

         return default( T );
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
