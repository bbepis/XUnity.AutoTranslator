### 2.8.0
 * Fixed whitespace handling to honor configuration more appropriately
 * IMGUI enabled by default, now supports numbers in translated texts
 * Support for overwriting IMGUI hook events

### 2.7.0
 * Additional installation instructions for standalone installation through ReiPatcher
 * Fixed a bug with NGUI that caused those texts not to be translated
 * Improved fix for gtrans (23.07.2018)

### 2.6.0
 * Fix for current issue with gtrans (23.07.2018)
 * Support for newer versions of unity engine (those including UnityEngine.CoreModule, etc. in Managed folder)
 * Keeps functioning if web services fails, but no requests will be sent in such scenario. Texts will simply be translated from cache
 * Concurrency now based on which type of endpoint. For gtrans it is set to 1
 * Changed hooking, such that if text framework fails, the others wont also fail
 * Bit more leniency in translation queue spam detection to prevent shutdown of plugin under normal circumstances

### 2.5.0
 * Various new rate limiting patterns to prevent spam to configured translate endpoint
 * Copy to clipboard feature

### 2.4.1
 * Disabled IMGUI hook due to bug

### 2.4.0
 * CHANGE - Completely reworked configuration for more organization
 * FEATURE - Added support for BaiduTranslate. User must provide AppId/AppSecret for API . Use "BaiduTranslate" as endpoint
 * FEATURE - Force splitting translated texts into newlines, if configured
 * BUG FIX - Fixed broken feature that allowed disabling the online Endpoint
 * BUG FIX - Eliminated potential concurrency issues that could cause a translated string to be retranslated.
 * BUG FIX - Better support for other 'from' languages than japanese, as the japanese symbol check has been replaced with a more generic one
 * BUG FIX - Fixed a bug where hot key actions (toggle translation, etc.) would often fail
 * BUG FIX - Multiline translations now partially supported. However, all texts considered dialogue will still be translated as a single line
 * BUG FIX - More leniency in allowing text formats (in translation files) to be included as translations

### 2.3.1
 * Fixed bug that caused the application to quit if any hooks were overriden.

### 2.3.0
 * Allow usage of SSL
 * Better dialogue caching handling. Often a dialogue might get translated multiple times because of small differences in the source text in regards to whitespace.

### 2.2.0
 * Added anti-spam safeguards to web requests that are sent. What it means: The plugin will no longer be able to attempt to translate a text it already considers translated.
 * Changed internal programmatic HTTP service provider from .NET WebClient to Unity WWW.

### 2.1.0
 * Fixed a bug that could cause a StackOverflowException in unfortunate scenarios, if other mods interfered.
 * Added configuration options to control which text frameworks to translate
 * Added integration feature that allows other translation plugins to use this plugin as a fallback
 * MUCH improved dialogue handling. Translations for dialogues should be significantly better than 2.0.1

### 2.0.1
 * Changed configuration path so to not conflict with the configuration files that other mods uses, as it does not use the shared configuration system. The previous version could override configuration from other mods.
 * General performance improvements.

### 2.0.0
 * The initial release
