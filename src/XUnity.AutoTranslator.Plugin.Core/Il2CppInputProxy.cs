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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

   private static Type _inputType;
   private static Type GetInputType()
   {
      _inputType ??= Type.GetType( "UnityEngine.Input, UnityEngine.CoreModule" );
      _inputType ??= Type.GetType( "UnityEngine.Input, UnityEngine.InputLegacyModule" );
      return _inputType ?? throw new InvalidOperationException( "Failed to find the UnityEngine.Input type" );
   }

   private static MethodInfo _getKey;
   public static bool GetKey( KeyCode key )
   {
      _getKey ??= GetInputType().GetMethod( "GetKey", [ typeof( KeyCode ) ] )!;
      return (bool)_getKey.Invoke( null, [ key ] )!;
   }

   private static MethodInfo _getKeyDown;
   public static bool GetKeyDown( KeyCode key )
   {
      _getKeyDown ??= GetInputType().GetMethod( "GetKeyDown", [ typeof( KeyCode ) ] )!;
      return (bool)_getKeyDown.Invoke( null, [ key ] )!;
   }

   private static MethodInfo _getMouseButton;
   public static bool GetMouseButton( int button )
   {
      _getMouseButton ??= GetInputType().GetMethod( "GetMouseButton", [ typeof( int ) ] )!;
      return (bool)_getMouseButton.Invoke( null, [ button ] )!;
   }

   private static MethodInfo _getMouseButtonDown;
   public static bool GetMouseButtonDown( int button )
   {
      _getMouseButtonDown ??= GetInputType().GetMethod( "GetMouseButtonDown", [ typeof( int ) ] )!;
      return (bool)_getMouseButtonDown.Invoke( null, [ button ] )!;
   }

   private static MethodInfo _resetInputAxes;
   public static void ResetInputAxes()
   {
      _resetInputAxes ??= GetInputType().GetMethod( "ResetInputAxes", [ ] )!;
      _resetInputAxes.Invoke( null, [ ] );
   }

   private static PropertyInfo _mouseScrollDelta;
   public static Vector2 mouseScrollDelta
   {
      get
      {
         _mouseScrollDelta ??= GetInputType().GetProperty( "mouseScrollDelta" )!;
         return (Vector2)_mouseScrollDelta.GetValue( null, null )!;
      }
   }

   private static PropertyInfo _mousePosition;
   public static Vector3 mousePosition
   {
      get
      {
         _mousePosition ??= GetInputType().GetProperty( "mousePosition" )!;
         return (Vector3)_mousePosition.GetValue( null, null )!;
      }
   }
}
#endif
