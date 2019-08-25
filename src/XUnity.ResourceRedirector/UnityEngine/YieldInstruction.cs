using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.Common.Logging;

namespace UnityEngineInternal
{
   internal class AsyncOperation : UnityEngine.YieldInstruction
   {
      internal IntPtr m_Ptr;
      
      private void InternalDestroy()
      {

      }

      public bool isDone
      {
         get
         {
            XuaLogger.ResourceRedirector.Error( "I am here!!!" );

            return false;
         }
      }

      public float progress
      {
         get
         {
            return 0.0f;
         }
      }

      public int priority
      {
         get
         {
            return 1;
         }
      }

      public bool allowSceneActivation
      {
         get
         {
            return true;
         }
      }
   }

   internal class AssetBundleRequest : AsyncOperation
   {
      public UnityEngine.Object asset
      {
         get
         {
            return null;
         }
      }

      public UnityEngine.Object[] allAssets
      {
         get
         {
            return null;
         }
      }
   }
}
