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
   public struct LayerMask
   {
      private int m_Mask;

      public int value
      {
         get
         {
            return m_Mask;
         }
         set
         {
            m_Mask = value;
         }
      }

      public static implicit operator int( LayerMask mask )
      {
         return mask.m_Mask;
      }

      public static implicit operator LayerMask( int intVal )
      {
         LayerMask result = default( LayerMask );
         result.m_Mask = intVal;
         return result;
      }

      public static extern string LayerToName( int layer );

      public static extern int NameToLayer( string layerName );

      public static int GetMask( params string[] layerNames )
      {
         if( layerNames == null )
         {
            throw new ArgumentNullException( "layerNames" );
         }

         int num = 0;
         foreach( string layerName in layerNames )
         {
            int num2 = NameToLayer( layerName );
            if( num2 != -1 )
            {
               num |= 1 << num2;
            }
         }

         return num;
      }
   }
}
