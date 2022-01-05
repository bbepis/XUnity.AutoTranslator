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
   public sealed class QualitySettings : Object
   {
      public QualitySettings( IntPtr pointer ) : base( IntPtr.Zero ) => throw new NotImplementedException();

      public static string[] names
      {
   
   
         get;
      }

      public static int pixelLightCount
      {
   
   
         get;
   
   
         set;
      }

      public static ShadowQuality shadows
      {
   
   
         get;
   
   
         set;
      }

      public static ShadowProjection shadowProjection
      {
   
   
         get;
   
   
         set;
      }

      public static int shadowCascades
      {
   
   
         get;
   
   
         set;
      }

      public static float shadowDistance
      {
   
   
         get;
   
   
         set;
      }

      public static ShadowResolution shadowResolution
      {
   
   
         get;
   
   
         set;
      }

      public static float shadowNearPlaneOffset
      {
   
   
         get;
   
   
         set;
      }

      public static float shadowCascade2Split
      {
   
   
         get;
   
   
         set;
      }

      public static Vector3 shadowCascade4Split
      {
         get
         {
            INTERNAL_get_shadowCascade4Split( out Vector3 value );
            return value;
         }
         set
         {
            INTERNAL_set_shadowCascade4Split( ref value );
         }
      }

      public static int masterTextureLimit
      {
   
   
         get;
   
   
         set;
      }

      public static AnisotropicFiltering anisotropicFiltering
      {
   
   
         get;
   
   
         set;
      }

      public static float lodBias
      {
   
   
         get;
   
   
         set;
      }

      public static int maximumLODLevel
      {
   
   
         get;
   
   
         set;
      }

      public static int particleRaycastBudget
      {
   
   
         get;
   
   
         set;
      }

      public static bool softParticles
      {
   
   
         get;
   
   
         set;
      }

      public static bool softVegetation
      {
   
   
         get;
   
   
         set;
      }

      public static bool realtimeReflectionProbes
      {
   
   
         get;
   
   
         set;
      }

      public static bool billboardsFaceCameraPosition
      {
   
   
         get;
   
   
         set;
      }

      public static int maxQueuedFrames
      {
   
   
         get;
   
   
         set;
      }

      public static int vSyncCount
      {
   
   
         get;
   
   
         set;
      }

      public static int antiAliasing
      {
   
   
         get;
   
   
         set;
      }

      public static ColorSpace desiredColorSpace
      {
   
   
         get;
      }

      public static ColorSpace activeColorSpace
      {
   
   
         get;
      }

      public static BlendWeights blendWeights
      {
   
   
         get;
   
   
         set;
      }

      public static int asyncUploadTimeSlice
      {
   
   
         get;
   
   
         set;
      }

      public static int asyncUploadBufferSize
      {
   
   
         get;
   
   
         set;
      }

      [Obsolete( "Use GetQualityLevel and SetQualityLevel", false )]
      public static QualityLevel currentLevel
      {
         get
         {
            return (QualityLevel)GetQualityLevel();
         }
         set
         {
            SetQualityLevel( (int)value, applyExpensiveChanges: true );
         }
      }



      public static extern int GetQualityLevel();



      public static extern void SetQualityLevel( int index, bool applyExpensiveChanges );


      public static void SetQualityLevel( int index )
      {
         bool applyExpensiveChanges = true;
         SetQualityLevel( index, applyExpensiveChanges );
      }



      public static extern void IncreaseLevel( bool applyExpensiveChanges );


      public static void IncreaseLevel()
      {
         bool applyExpensiveChanges = false;
         IncreaseLevel( applyExpensiveChanges );
      }



      public static extern void DecreaseLevel( bool applyExpensiveChanges );


      public static void DecreaseLevel()
      {
         bool applyExpensiveChanges = false;
         DecreaseLevel( applyExpensiveChanges );
      }



      private static extern void INTERNAL_get_shadowCascade4Split( out Vector3 value );



      private static extern void INTERNAL_set_shadowCascade4Split( ref Vector3 value );
   }
}
