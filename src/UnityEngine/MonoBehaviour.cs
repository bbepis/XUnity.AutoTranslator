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
   public class MonoBehaviour : Behaviour
   {
      public MonoBehaviour( IntPtr ptr )
      {

      }

      public MonoBehaviour()
      {

      }

#if IL2CPP
      public Coroutine StartCoroutine( Il2CppSystem.Collections.IEnumerator routine ) => throw new NotImplementedException();
#else
      public Coroutine StartCoroutine( IEnumerator routine ) => throw new NotImplementedException();
#endif

      public Coroutine StartCoroutine( string methodName, object value ) => throw new NotImplementedException();

      public Coroutine StartCoroutine( string methodName ) => throw new NotImplementedException();

      public void StopCoroutine( string methodName ) => throw new NotImplementedException();

      public void StopCoroutine( IEnumerator routine ) => throw new NotImplementedException();

      public void StopCoroutine( Coroutine routine ) => throw new NotImplementedException();
   }
}
