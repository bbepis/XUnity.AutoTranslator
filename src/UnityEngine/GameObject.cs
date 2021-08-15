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
   public class GameObject : Object
   {
      public Transform transform
      {


         get;
      }

      public int layer
      {


         get;


         set;
      }

      [Obsolete( "GameObject.active is obsolete. Use GameObject.SetActive(), GameObject.activeSelf or GameObject.activeInHierarchy." )]
      public bool active
      {


         get;


         set;
      }

      public bool activeSelf
      {


         get;
      }

      public bool activeInHierarchy
      {


         get;
      }

      public bool isStatic
      {


         get;


         set;
      }

      internal bool isStaticBatchable
      {


         get;
      }

      public string tag
      {


         get;


         set;
      }

      public Scene scene
      {
         get
         {
            INTERNAL_get_scene( out Scene value );
            return value;
         }
      }

      public GameObject gameObject => this;

      public GameObject( string name )
      {
         Internal_CreateGameObject( this, name );
      }

      public GameObject()
      {
         Internal_CreateGameObject( this, null );
      }

      public GameObject( string name, params Type[] components )
      {
         Internal_CreateGameObject( this, name );
         foreach( Type componentType in components )
         {
            AddComponent( componentType );
         }
      }



      public static extern GameObject CreatePrimitive( PrimitiveType type );




      public extern Component GetComponent( Type type );



      internal extern void GetComponentFastPath( Type type, IntPtr oneFurtherThanResultValue );

      public T GetComponent<T>() => throw new NotImplementedException();



      internal extern Component GetComponentByName( string type );

      public Component GetComponent( string type )
      {
         return GetComponentByName( type );
      }




      public extern Component GetComponentInChildren( Type type, bool includeInactive );


      public Component GetComponentInChildren( Type type )
      {
         return GetComponentInChildren( type, includeInactive: false );
      }

      public T GetComponentInChildren<T>()
      {
         bool includeInactive = false;
         return GetComponentInChildren<T>( includeInactive );
      }

      public T GetComponentInChildren<T>( bool includeInactive )
      {
         return (T)(object)GetComponentInChildren( typeof( T ), includeInactive );
      }




      public extern Component GetComponentInParent( Type type );

      public T GetComponentInParent<T>()
      {
         return (T)(object)GetComponentInParent( typeof( T ) );
      }

      public Component[] GetComponents( Type type )
      {
         return (Component[])GetComponentsInternal( type, useSearchTypeAsArrayReturnType: false, recursive: false, includeInactive: true, reverse: false, null );
      }

      public T[] GetComponents<T>()
      {
         return (T[])GetComponentsInternal( typeof( T ), useSearchTypeAsArrayReturnType: true, recursive: false, includeInactive: true, reverse: false, null );
      }

      public void GetComponents( Type type, List<Component> results )
      {
         GetComponentsInternal( type, useSearchTypeAsArrayReturnType: false, recursive: false, includeInactive: true, reverse: false, results );
      }

      public void GetComponents<T>( List<T> results )
      {
         GetComponentsInternal( typeof( T ), useSearchTypeAsArrayReturnType: false, recursive: false, includeInactive: true, reverse: false, results );
      }

      public Component[] GetComponentsInChildren( Type type )
      {
         bool includeInactive = false;
         return GetComponentsInChildren( type, includeInactive );
      }

      public Component[] GetComponentsInChildren( Type type, bool includeInactive ) => throw new NotImplementedException();

      public T[] GetComponentsInChildren<T>( bool includeInactive )
      {
         return (T[])GetComponentsInternal( typeof( T ), useSearchTypeAsArrayReturnType: true, recursive: true, includeInactive, reverse: false, null );
      }

      public void GetComponentsInChildren<T>( bool includeInactive, List<T> results )
      {
         GetComponentsInternal( typeof( T ), useSearchTypeAsArrayReturnType: true, recursive: true, includeInactive, reverse: false, results );
      }

      public T[] GetComponentsInChildren<T>()
      {
         return GetComponentsInChildren<T>( includeInactive: false );
      }

      public void GetComponentsInChildren<T>( List<T> results )
      {
         GetComponentsInChildren( includeInactive: false, results );
      }

      public Component[] GetComponentsInParent( Type type )
      {
         bool includeInactive = false;
         return GetComponentsInParent( type, includeInactive );
      }

      public Component[] GetComponentsInParent( Type type, bool includeInactive ) => throw new NotImplementedException();

      public void GetComponentsInParent<T>( bool includeInactive, List<T> results )
      {
         GetComponentsInternal( typeof( T ), useSearchTypeAsArrayReturnType: true, recursive: true, includeInactive, reverse: true, results );
      }

      public T[] GetComponentsInParent<T>( bool includeInactive )
      {
         return (T[])GetComponentsInternal( typeof( T ), useSearchTypeAsArrayReturnType: true, recursive: true, includeInactive, reverse: true, null );
      }

      public T[] GetComponentsInParent<T>()
      {
         return GetComponentsInParent<T>( includeInactive: false );
      }



      private extern Array GetComponentsInternal( Type type, bool useSearchTypeAsArrayReturnType, bool recursive, bool includeInactive, bool reverse, object resultList );



      internal extern Component AddComponentInternal( string className );



      public extern void SetActive( bool value );


      [Obsolete( "gameObject.SetActiveRecursively() is obsolete. Use GameObject.SetActive(), which is now inherited by children." )]

      public extern void SetActiveRecursively( bool state );



      public extern bool CompareTag( string tag );



      public static extern GameObject FindGameObjectWithTag( string tag );

      public static GameObject FindWithTag( string tag )
      {
         return FindGameObjectWithTag( tag );
      }



      public static extern GameObject[] FindGameObjectsWithTag( string tag );


      public void SendMessageUpwards( string methodName, object value ) => throw new NotImplementedException();

      public void SendMessageUpwards( string methodName ) => throw new NotImplementedException();




      public void SendMessage( string methodName, object value ) => throw new NotImplementedException();

      public void SendMessage( string methodName ) => throw new NotImplementedException();




      public void BroadcastMessage( string methodName, object parameter ) => throw new NotImplementedException();

      public void BroadcastMessage( string methodName ) => throw new NotImplementedException();



      private extern Component Internal_AddComponentWithType( Type componentType );


      public Component AddComponent( Type componentType )
      {
         return Internal_AddComponentWithType( componentType );
      }

      public T AddComponent<T>() where T : Component
      {
         return AddComponent( typeof( T ) ) as T;
      }



      private static extern void Internal_CreateGameObject( GameObject mono, string name );



      public static extern GameObject Find( string name );



      private extern void INTERNAL_get_scene( out Scene value );
   }
}
