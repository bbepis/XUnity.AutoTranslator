using System;
using System.Collections.Generic;

namespace XUnity.ResourceRedirector
{
   internal static class PrioritizedItem
   {
      public static PrioritizedItem<TItem> Create<TItem>( TItem item, int priority )
      {
         return new PrioritizedItem<TItem>( item, priority );
      }
   }

   internal class PrioritizedItem<TItem> : IComparable<PrioritizedItem<TItem>>, IEquatable<PrioritizedItem<TItem>>
   {
      public PrioritizedItem( TItem item, int priority )
      {
         Item = item;
         Priority = priority;
      }

      public TItem Item { get; }

      public int Priority { get; }

      public int CompareTo( PrioritizedItem<TItem> other )
      {
         return other.Priority.CompareTo( Priority );
      }

      public override bool Equals( object obj )
      {
         return Equals( obj as PrioritizedItem<TItem> );
      }

      public bool Equals( PrioritizedItem<TItem> other )
      {
         return EqualityComparer<TItem>.Default.Equals( Item, other.Item );
      }

      public override int GetHashCode()
      {
         var hashCode = -1406788065;
         hashCode = hashCode * -1521134295 + EqualityComparer<TItem>.Default.GetHashCode( Item );
         return hashCode;
      }
   }
}
