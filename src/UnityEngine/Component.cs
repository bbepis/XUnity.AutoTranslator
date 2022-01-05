using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   public class Component : Object
   {
      public Component( IntPtr pointer ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public Transform transform => throw new NotImplementedException();

      public GameObject gameObject => throw new NotImplementedException();

      public string tag
      {
         get => throw new NotImplementedException();
         set => throw new NotImplementedException();
      }

#if IL2CPP
      public Component GetComponent( Il2CppSystem.Type type ) => throw new NotImplementedException();
#else
      public Component GetComponent( Type type ) => throw new NotImplementedException();
#endif

      public T GetComponent<T>() => throw new NotImplementedException();

      public Component GetComponent( string type ) => throw new NotImplementedException();

      public Component GetComponentInChildren( Type t, bool includeInactive ) => throw new NotImplementedException();

      public Component GetComponentInChildren( Type t ) => throw new NotImplementedException();

      public T GetComponentInChildren<T>() => throw new NotImplementedException();

      public T GetComponentInChildren<T>( bool includeInactive ) => throw new NotImplementedException();

      public Component[] GetComponentsInChildren( Type t ) => throw new NotImplementedException();

      public Component[] GetComponentsInChildren( Type t, bool includeInactive ) => throw new NotImplementedException();

      public T[] GetComponentsInChildren<T>( bool includeInactive ) => throw new NotImplementedException();

      public void GetComponentsInChildren<T>( bool includeInactive, List<T> result ) => throw new NotImplementedException();

      public T[] GetComponentsInChildren<T>() => throw new NotImplementedException();

      public void GetComponentsInChildren<T>( List<T> results ) => throw new NotImplementedException();

      public Component GetComponentInParent( Type t ) => throw new NotImplementedException();

      public T GetComponentInParent<T>() => throw new NotImplementedException();

      public Component[] GetComponentsInParent( Type t ) => throw new NotImplementedException();

      public Component[] GetComponentsInParent( Type t, bool includeInactive ) => throw new NotImplementedException();

      public T[] GetComponentsInParent<T>( bool includeInactive ) => throw new NotImplementedException();

      public void GetComponentsInParent<T>( bool includeInactive, List<T> results ) => throw new NotImplementedException();

      public T[] GetComponentsInParent<T>() => throw new NotImplementedException();

      public Component[] GetComponents( Type type ) => throw new NotImplementedException();

      public void GetComponents( Type type, List<Component> results ) => throw new NotImplementedException();

      public void GetComponents<T>( List<T> results ) => throw new NotImplementedException();

      public T[] GetComponents<T>() => throw new NotImplementedException();

      public bool CompareTag( string tag ) => throw new NotImplementedException();

      public void SendMessageUpwards( string methodName, object value ) => throw new NotImplementedException();

      public void SendMessageUpwards( string methodName ) => throw new NotImplementedException();

      public void SendMessage( string methodName, object value ) => throw new NotImplementedException();

      public void SendMessage( string methodName ) => throw new NotImplementedException();

      public void BroadcastMessage( string methodName, object parameter ) => throw new NotImplementedException();

      public void BroadcastMessage( string methodName ) => throw new NotImplementedException();
   }
}
