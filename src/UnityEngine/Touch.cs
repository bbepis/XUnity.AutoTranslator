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
   public struct Touch
   {
      private int m_FingerId;

      private Vector2 m_Position;

      private Vector2 m_RawPosition;

      private Vector2 m_PositionDelta;

      private float m_TimeDelta;

      private int m_TapCount;

      private TouchPhase m_Phase;

      private TouchType m_Type;

      private float m_Pressure;

      private float m_maximumPossiblePressure;

      private float m_Radius;

      private float m_RadiusVariance;

      private float m_AltitudeAngle;

      private float m_AzimuthAngle;

      public int fingerId
      {
         get
         {
            return m_FingerId;
         }
         set
         {
            m_FingerId = value;
         }
      }

      public Vector2 position
      {
         get
         {
            return m_Position;
         }
         set
         {
            m_Position = value;
         }
      }

      public Vector2 rawPosition
      {
         get
         {
            return m_RawPosition;
         }
         set
         {
            m_RawPosition = value;
         }
      }

      public Vector2 deltaPosition
      {
         get
         {
            return m_PositionDelta;
         }
         set
         {
            m_PositionDelta = value;
         }
      }

      public float deltaTime
      {
         get
         {
            return m_TimeDelta;
         }
         set
         {
            m_TimeDelta = value;
         }
      }

      public int tapCount
      {
         get
         {
            return m_TapCount;
         }
         set
         {
            m_TapCount = value;
         }
      }

      public TouchPhase phase
      {
         get
         {
            return m_Phase;
         }
         set
         {
            m_Phase = value;
         }
      }

      public float pressure
      {
         get
         {
            return m_Pressure;
         }
         set
         {
            m_Pressure = value;
         }
      }

      public float maximumPossiblePressure
      {
         get
         {
            return m_maximumPossiblePressure;
         }
         set
         {
            m_maximumPossiblePressure = value;
         }
      }

      public TouchType type
      {
         get
         {
            return m_Type;
         }
         set
         {
            m_Type = value;
         }
      }

      public float altitudeAngle
      {
         get
         {
            return m_AltitudeAngle;
         }
         set
         {
            m_AltitudeAngle = value;
         }
      }

      public float azimuthAngle
      {
         get
         {
            return m_AzimuthAngle;
         }
         set
         {
            m_AzimuthAngle = value;
         }
      }

      public float radius
      {
         get
         {
            return m_Radius;
         }
         set
         {
            m_Radius = value;
         }
      }

      public float radiusVariance
      {
         get
         {
            return m_RadiusVariance;
         }
         set
         {
            m_RadiusVariance = value;
         }
      }
   }
}
