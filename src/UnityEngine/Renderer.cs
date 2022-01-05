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
   public class Renderer : Component
   {
      public Renderer() : base( IntPtr.Zero ) => throw new NotImplementedException();

      internal Transform staticBatchRootTransform
      {
         get;
         set;
      }

      internal int staticBatchIndex
      {
         get;
      }

      public bool isPartOfStaticBatch
      {
         get;
      }

      public Matrix4x4 worldToLocalMatrix
      {
         get
         {
            INTERNAL_get_worldToLocalMatrix( out Matrix4x4 value );
            return value;
         }
      }

      public Matrix4x4 localToWorldMatrix
      {
         get
         {
            INTERNAL_get_localToWorldMatrix( out Matrix4x4 value );
            return value;
         }
      }

      public bool enabled
      {
         get;
         set;
      }

      public ShadowCastingMode shadowCastingMode
      {
         get;
         set;
      }

      public bool receiveShadows
      {
         get;
         set;
      }

      public Material material
      {
         get;
         set;
      }

      public Material sharedMaterial
      {
         get;
         set;
      }

      public Material[] materials
      {
         get;
         set;
      }

      public Material[] sharedMaterials
      {
         get;
         set;
      }

      public Bounds bounds
      {
         get
         {
            INTERNAL_get_bounds( out Bounds value );
            return value;
         }
      }

      public int lightmapIndex
      {
         get;
         set;
      }

      public int realtimeLightmapIndex
      {
         get;
         set;
      }

      public Vector4 lightmapScaleOffset
      {
         get
         {
            INTERNAL_get_lightmapScaleOffset( out Vector4 value );
            return value;
         }
         set
         {
            INTERNAL_set_lightmapScaleOffset( ref value );
         }
      }

      public MotionVectorGenerationMode motionVectorGenerationMode
      {
         get;
         set;
      }

      public Vector4 realtimeLightmapScaleOffset
      {
         get
         {
            INTERNAL_get_realtimeLightmapScaleOffset( out Vector4 value );
            return value;
         }
         set
         {
            INTERNAL_set_realtimeLightmapScaleOffset( ref value );
         }
      }

      public bool isVisible
      {
         get;
      }

      public LightProbeUsage lightProbeUsage
      {
         get;
         set;
      }

      public GameObject lightProbeProxyVolumeOverride
      {
         get;
         set;
      }

      public Transform probeAnchor
      {
         get;
         set;
      }

      public ReflectionProbeUsage reflectionProbeUsage
      {
         get;
         set;
      }

      public string sortingLayerName
      {
         get;
         set;
      }

      public int sortingLayerID
      {
         get;
         set;
      }

      public int sortingOrder
      {
         get;
         set;
      }

      internal int sortingGroupID
      {
         get;
      }

      internal int sortingGroupOrder
      {
         get;
      }

      [Obsolete( "Use shadowCastingMode instead.", false )]
      public bool castShadows
      {
         get
         {
            return shadowCastingMode != ShadowCastingMode.Off;
         }
         set
         {
            shadowCastingMode = ( value ? ShadowCastingMode.On : ShadowCastingMode.Off );
         }
      }

      [Obsolete( "Use motionVectorGenerationMode instead.", false )]
      public bool motionVectors
      {
         get
         {
            return motionVectorGenerationMode == MotionVectorGenerationMode.Object;
         }
         set
         {
            motionVectorGenerationMode = ( value ? MotionVectorGenerationMode.Object : MotionVectorGenerationMode.Camera );
         }
      }

      [Obsolete( "Use lightProbeUsage instead.", false )]
      public bool useLightProbes
      {
         get
         {
            return lightProbeUsage != LightProbeUsage.Off;
         }
         set
         {
            lightProbeUsage = ( value ? LightProbeUsage.BlendProbes : LightProbeUsage.Off );
         }
      }

      internal extern void SetStaticBatchInfo( int firstSubMesh, int subMeshCount );

      private extern void INTERNAL_get_worldToLocalMatrix( out Matrix4x4 value );

      private extern void INTERNAL_get_localToWorldMatrix( out Matrix4x4 value );

      private extern void INTERNAL_get_bounds( out Bounds value );

      private extern void INTERNAL_get_lightmapScaleOffset( out Vector4 value );

      private extern void INTERNAL_set_lightmapScaleOffset( ref Vector4 value );

      private extern void INTERNAL_get_realtimeLightmapScaleOffset( out Vector4 value );

      private extern void INTERNAL_set_realtimeLightmapScaleOffset( ref Vector4 value );

      private extern void GetClosestReflectionProbesInternal( object result );
   }
}
