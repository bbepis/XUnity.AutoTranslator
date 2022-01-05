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
   [StructLayout( LayoutKind.Sequential )]
   public class ScriptableObject : Object
   {
      public ScriptableObject( IntPtr pointer ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public ScriptableObject() : base( IntPtr.Zero ) => throw new NotImplementedException();

      private static extern void Internal_CreateScriptableObject( ScriptableObject self );

      [Obsolete( "Use EditorUtility.SetDirty instead" )]
      public void SetDirty()
      {
         INTERNAL_CALL_SetDirty( this );
      }

      private static extern void INTERNAL_CALL_SetDirty( ScriptableObject self );

      public static extern ScriptableObject CreateInstance( string className );

      public static ScriptableObject CreateInstance( Type type )
      {
         return CreateInstanceFromType( type );
      }

      private static extern ScriptableObject CreateInstanceFromType( Type type );

      public static T CreateInstance<T>() where T : ScriptableObject
      {
         return (T)CreateInstance( typeof( T ) );
      }
   }
}
