using System;
using System.Collections.Generic;
using System.Linq;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class ExtensionDataHelper
   {
      private static readonly object Sync = new object();
      private static readonly WeakDictionary<object, object> DynamicFields = new WeakDictionary<object, object>();

      static ExtensionDataHelper()
      {
         MaintenanceHelper.AddMaintenanceFunction( Cull, 12 );
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static int WeakReferenceCount
      {
         get
         {
            lock( Sync )
            {
               return DynamicFields.Count;
            }
         }
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="obj"></param>
      /// <param name="t"></param>
      public static void SetExtensionData<T>( this object obj, T t )
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

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="obj"></param>
      /// <returns></returns>
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

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="obj"></param>
      /// <returns></returns>
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

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static void Cull()
      {
         lock( Sync )
         {
            DynamicFields.RemoveCollectedEntries();
         }
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <returns></returns>
      public static List<KeyValuePair<object, object>> GetAllRegisteredObjects()
      {
         lock( Sync )
         {
            return IterateAllPairs().ToList();
         }
      }

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      /// <param name="obj"></param>
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
