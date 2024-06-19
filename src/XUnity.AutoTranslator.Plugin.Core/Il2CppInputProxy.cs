#if IL2CPP

using System;
using System.Reflection;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core;

/// <summary>
/// UnityEngine.Input is usually in UnityEngine or UnityEngine.CoreModule (in which case there is a TypeForwardedTo attribute for it in UnityEngine),
/// but in newer games it can be in UnityEngine.InputLegacyModule with no type forwarding. In this case it's necessary to load from the legacy assembly manually.
///
/// Currently slow relection is used to resolve UnityEngine.Input in both UnityEngine.CoreModule and UnityEngine.InputLegacyModule.
///
/// This is a pretty dirty solution, but it is very simple and self-contained so it will suffice for now.
/// Ideally there would be a full abstraction class that also handles Unity.InputSystem like BepInEx.UnityInput.
///
/// TODO: Change all input handling to a local copy of BepInEx.UnityInput whenever it's ported to the master branch
/// </summary>
public static class Input
{
  static Type type = null;
  static Type GetInputType()
  {
    type ??= Type.GetType("UnityEngine.Input, UnityEngine.CoreModule");
    type ??= Type.GetType("UnityEngine.Input, UnityEngine.InputLegacyModule");
    return type ?? throw new InvalidOperationException("Failed to find the UnityEngine.Input type");
  }

  static MethodInfo _getKey = null;
  public static bool GetKey(KeyCode key)
  {
    _getKey ??= GetInputType().GetMethod("GetKey", new Type[] { typeof(KeyCode) });
    return (bool)_getKey.Invoke(null, new object[] { key });
  }
  static MethodInfo _getKeyDown = null;
  public static bool GetKeyDown(KeyCode key)
  {
    _getKeyDown ??= GetInputType().GetMethod("GetKeyDown", new Type[] { typeof(KeyCode) });
    return (bool)_getKeyDown.Invoke(null, new object[] { key });
  }

  static MethodInfo _getMouseButton = null;
  public static bool GetMouseButton(int button){
    _getMouseButton ??= GetInputType().GetMethod("GetMouseButton", new Type[] { typeof(int) });
    return (bool)_getMouseButton.Invoke(null, new object[] { button });
  }

  static MethodInfo _getMouseButtonDown = null;
  public static bool GetMouseButtonDown(int button){
    _getMouseButtonDown ??= GetInputType().GetMethod("GetMouseButtonDown", new Type[] { typeof(int) });
    return (bool)_getMouseButtonDown.Invoke(null, new object[] { button });
  }

  static MethodInfo resetInputAxes = null;
  public static void ResetInputAxes(){
    resetInputAxes ??= GetInputType().GetMethod("ResetInputAxes", new Type[] {});
    resetInputAxes.Invoke(null, new object[] {});
  }
  
  static PropertyInfo _mouseScrollDelta = null;
  public static Vector2 mouseScrollDelta {
    get {
      // UnityEngine.Input.GetMouseButtonDown
      _mouseScrollDelta ??= GetInputType().GetProperty("mouseScrollDelta");
      return (Vector2)_mouseScrollDelta.GetValue(null, null);
    }
  }

  static PropertyInfo _mousePosition = null;
  public static Vector3 mousePosition {
    get {
      _mousePosition ??= GetInputType().GetProperty("mousePosition");
      return (Vector3)_mousePosition.GetValue(null, null);
    }
  }
}
#endif
