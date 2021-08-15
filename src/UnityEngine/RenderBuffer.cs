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
   public struct RenderBuffer
   {
      internal int m_RenderTextureInstanceID;

      internal IntPtr m_BufferPtr;

      internal RenderBufferLoadAction loadAction
      {
         get => throw new NotImplementedException();
         set
         {
            SetLoadAction( value );
         }
      }

      internal RenderBufferStoreAction storeAction
      {
         get => throw new NotImplementedException();
         set
         {
            SetStoreAction( value );
         }
      }

      internal void SetLoadAction( RenderBufferLoadAction action ) => throw new NotImplementedException();

      internal void SetStoreAction( RenderBufferStoreAction action ) => throw new NotImplementedException();

      public IntPtr GetNativeRenderBufferPtr() => throw new NotImplementedException();
   }
}
