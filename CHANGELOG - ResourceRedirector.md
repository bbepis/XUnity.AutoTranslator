### 2.1.0
 * FEATURE - Added support for latest MelonLoader and BepInEx bleeding edge builds (only for IL2CPP, use stable release for Mono), also dropped support for earlier versions!

### 2.0.0
 * FEATURE - IL2CPP support through BepInEx 6 and MelonLoader
 * MISC - Prioritize HarmonyX hooks over MonoMod hooks
 * MISC - Changed BepInEx setting names
 * MISC - Renamed BepInEx plugin package from BepIn-5x to BepInEx. If updating remember to remove the old BepIn-5x DLL
 * BUG FIX - Fixed bug that caused some rarely used methods to be short-circuited incorrectly

### 1.2.1
 * MISC - Improved logging statements for redirection subscriptions allowing for less spam

### 1.2.0
 * FEATURE - Support callbacks for asset bundles loaded from streams

### 1.1.3
 * BUG FIX - Fixed bug that caused the GetBuiltinResource to not be found on later versions of the Unity Engine. Thanks to aqie13 on GitHub

### 1.1.2
 * MISC - Internal API changes - some code moved to XUnity.Common

### 1.1.1
 * BUG FIX - Development-time fix to the nuget package
 * BUG FIX - Fixed bug in method 'LoadFromFileWithRandomizedCabIfRequired' where offset was ignored if the initial load attempt failed

### 1.1.0
 * FEATURE - Added method to load an asset from a file, with fallback to loading it from an in-memory stream with randomized CAB
 * FEATURE - Added support for postfix hooks on AssetBundle loads
 * FEATURE - Added hook for 'LoadFromMemory' and 'LoadFromMemoryAsync' when loading asset bundles

### 1.0.1
 * MISC - Support for UnityEngine version 2018.2 and lower

### 1.0.0
 * Initial release
