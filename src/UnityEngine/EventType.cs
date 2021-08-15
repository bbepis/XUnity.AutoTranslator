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
   public enum EventType
   {
      MouseDown = 0,
      MouseUp = 1,
      MouseMove = 2,
      MouseDrag = 3,
      KeyDown = 4,
      KeyUp = 5,
      ScrollWheel = 6,
      Repaint = 7,
      Layout = 8,
      DragUpdated = 9,
      DragPerform = 10,
      DragExited = 0xF,
      Ignore = 11,
      Used = 12,
      ValidateCommand = 13,
      ExecuteCommand = 14,
      ContextClick = 0x10,
      MouseEnterWindow = 20,
      MouseLeaveWindow = 21,
      mouseDown = 0,
      mouseUp = 1,
      mouseMove = 2,
      mouseDrag = 3,
      keyDown = 4,
      keyUp = 5,
      scrollWheel = 6,
      repaint = 7,
      layout = 8,
      dragUpdated = 9,
      dragPerform = 10,
      ignore = 11,
      used = 12
   }
}
