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
