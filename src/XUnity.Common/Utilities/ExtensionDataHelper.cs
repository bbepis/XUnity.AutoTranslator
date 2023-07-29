using System;
using System.Collections.Generic;
using System.Linq;
using XUnity.Common.Extensions;

#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
#endif

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class ExtensionDataHelper
   {
#if IL2CPP
      private static readonly Dictionary<Il2CppObjectBase, object> DynamicFields = new Dictionary<Il2CppObjectBase, object>( UnityObjectReferenceComparer.Default );
#endif

      private static readonly object Sync = new object();
      private static readonly WeakDictionary<object, object> WeakDynamicFields = new WeakDictionary<object, object>();

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
               return WeakDynamicFields.Count
#if IL2CPP
                  + DynamicFields.Count
#endif
                  ;
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
#if IL2CPP
            if( obj is Il2CppObjectBase obj2 )
            {
               if( DynamicFields.TryGetValue( obj2, out object value ) )
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
                     DynamicFields[ obj2 ] = newDictionary;
                  }
               }
               else
               {
                  DynamicFields[ obj2 ] = t;
               }
            }
            else
#endif
            {
               if( WeakDynamicFields.TryGetValue( obj, out object value ) )
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
                     WeakDynamicFields[ obj ] = newDictionary;
                  }
               }
               else
               {
                  WeakDynamicFields[ obj ] = t;
               }
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
#if IL2CPP
            if( obj is Il2CppObjectBase obj2 )
            {
               if( DynamicFields.TryGetValue( obj2, out object value ) )
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
                     DynamicFields[ obj2 ] = newDictionary;
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
                  DynamicFields[ obj2 ] = t;
                  return t;
               }
            }
            else
#endif
            {
               if( WeakDynamicFields.TryGetValue( obj, out object value ) )
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
                     WeakDynamicFields[ obj ] = newDictionary;
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
                  WeakDynamicFields[ obj ] = t;
                  return t;
               }
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
      {
         if( obj == null ) return default( T );

         lock( Sync )
         {
#if IL2CPP
            if( obj is Il2CppObjectBase obj2 )
            {
               if( DynamicFields.TryGetValue( obj2, out object value ) )
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
            else
#endif
            {
               if( WeakDynamicFields.TryGetValue( obj, out object value ) )
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
            WeakDynamicFields.RemoveCollectedEntries();

#if IL2CPP
            List<Il2CppObjectBase> toRemove = null;
            foreach( var pair in DynamicFields )
            {
               var key = pair.Key;

               if( key is Il2CppObjectBase gc && gc.IsCollected() )
               {
                  if( toRemove == null )
                     toRemove = new List<Il2CppObjectBase>();

                  toRemove.Add( gc );
               }
            }

            if( toRemove != null )
            {
               foreach( var key in toRemove )
                  DynamicFields.Remove( key );
            }
#endif
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
#if IL2CPP
            if(obj is Il2CppObjectBase collectable)
            {
               DynamicFields.Remove( collectable );
            }
            else
#endif
            {
               WeakDynamicFields.Remove( obj );
            }
         }
      }

      private static IEnumerable<KeyValuePair<object, object>> IterateAllPairs()
      {
         foreach( var kvp in WeakDynamicFields )
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

#if IL2CPP
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
               yield return new KeyValuePair<object, object>( kvp.Key, kvp.Value );
            }
         }
#endif
      }
   }
}
