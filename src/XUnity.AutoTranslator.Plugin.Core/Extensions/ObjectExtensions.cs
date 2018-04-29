using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   public static class ObjectExtensions
   {
      private static readonly object Sync = new object();
      private static readonly WeakDictionary<object, object> DynamicFields = new WeakDictionary<object, object>();

      public static TranslationInfo GetTranslationInfo( this object obj, bool isAwakening )
      {
         if( obj is GUIContent ) return null;

         var info = obj.Get<TranslationInfo>();

         info.IsAwake = info.IsAwake || isAwakening;

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

      public static IEnumerable<KeyValuePair<object, object>> GetAllRegisteredObjects()
      {
         lock( Sync )
         {
            return DynamicFields.ToList();
         }
      }
   }
}
