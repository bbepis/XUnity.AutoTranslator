using System;
using System.Collections.Generic;

namespace XUnity.ResourceRedirector
{
   internal static class PrioritizedCallback
   {
      public static PrioritizedCallback<TCallback> Create<TCallback>( TCallback item, int priority )
      {
         return new PrioritizedCallback<TCallback>( item, priority );
      }
   }

   internal class PrioritizedCallback<TCallback> : IComparable<PrioritizedCallback<TCallback>>, IEquatable<PrioritizedCallback<TCallback>>
   {
      public PrioritizedCallback( TCallback callback, int priority )
      {
         Callback = callback;
         Priority = priority;
      }

      public TCallback Callback { get; }

      public int Priority { get; }

      public bool IsBeingCalled { get; set; }

      public int CompareTo( PrioritizedCallback<TCallback> other )
      {
         return other.Priority.CompareTo( Priority );
      }

      public override bool Equals( object obj )
      {
         return Equals( obj as PrioritizedCallback<TCallback> );
      }

      public bool Equals( PrioritizedCallback<TCallback> other )
      {
         return EqualityComparer<TCallback>.Default.Equals( Callback, other.Callback );
      }

      public override int GetHashCode()
      {
         var hashCode = -1406788065;
         hashCode = hashCode * -1521134295 + EqualityComparer<TCallback>.Default.GetHashCode( Callback );
         return hashCode;
      }
   }
}
