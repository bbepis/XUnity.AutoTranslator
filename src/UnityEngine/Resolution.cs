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
   public struct Resolution
   {
      private int m_Width;

      private int m_Height;

      private int m_RefreshRate;

      public int width
      {
         get
         {
            return m_Width;
         }
         set
         {
            m_Width = value;
         }
      }

      public int height
      {
         get
         {
            return m_Height;
         }
         set
         {
            m_Height = value;
         }
      }

      public int refreshRate
      {
         get
         {
            return m_RefreshRate;
         }
         set
         {
            m_RefreshRate = value;
         }
      }

      public override string ToString() => throw new NotImplementedException();
   }
}
