using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// One-click AR setup: Lucille's Journey > AR Tabletop > Set Up Everything.
    ///
    /// Steps (it resumes by itself across the recompiles Unity does in between):
    ///   1. install AR Foundation + ARCore + ARKit
    ///   2. define LJ_AR, which switches the AR scripts on (they are inert without AR packages, so the project always compiles)
    ///   3. Android/iOS player settings AR needs
    ///   4. enable the ARCore / ARKit XR loaders
    ///   5. add the AR camera-background feature to the URP renderer(s)
    ///
    /// Nothing here references AR types directly (only by name), so this file compiles before any package exists.
    /// There is no scene to edit: the AR rig is built in code at runtime when the player picks "Play in AR".
    /// </summary>
    [InitializeOnLoad]
    public static class ARTabletopSetup
    {
        public const string Define = "LJ_AR";
        private const string RunningFlag = "LJ_AR_SETUP_RUNNING";
        private const string ModePrefKey = "lj_ar_requested"; // same key as ARMode

        private static readonly string[] Packages = { "com.unity.xr.arfoundation", "com.unity.xr.arcore", "com.unity.xr.arkit" };
        private static readonly NamedBuildTarget[] DefineTargets = { NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone };

        private static AddAndRemoveRequest request;
        private static double deadline;
        private static bool waitingLogged;

        static ARTabletopSetup()
        {
            // After a domain reload in the middle of a run, carry on.
            if (SessionState.GetBool(RunningFlag, false)) EditorApplication.delayCall += Resume;
        }

        // ------------------------------------------------------------------------------------------------------------
        // Menu
        // ------------------------------------------------------------------------------------------------------------

        [MenuItem("Lucille's Journey/AR Tabletop/Set Up Everything")]
        public static void RunEverything()
        {
            SessionState.SetBool(RunningFlag, true);
            Log("Starting AR tabletop setup...");
            Resume();
        }

        [MenuItem("Lucille's Journey/AR Tabletop/Setup Window")]
        public static void OpenWindow() => EditorWindow.GetWindow<ARSetupWindow>("AR Setup").Show();

        private const string RequestMenu = "Lucille's Journey/AR Tabletop/Start Game In AR Mode (editor testing)";

        [MenuItem(RequestMenu)]
        public static void ToggleRequest() => PlayerPrefs.SetInt(ModePrefKey, ModeRequested ? 0 : 1);

        [MenuItem(RequestMenu, true)]
        private static bool ToggleRequestValidate()
        {
            Menu.SetChecked(RequestMenu, ModeRequested);
            return true;
        }

        private const string TouchMenu = "Lucille's Journey/Touch Controls/Show In Editor (testing)";
        private const string TouchPrefKey = "lj_touch_controls";

        [MenuItem(TouchMenu)]
        public static void ToggleTouchControls() => PlayerPrefs.SetInt(TouchPrefKey, PlayerPrefs.GetInt(TouchPrefKey, 0) == 1 ? 0 : 1);

        [MenuItem(TouchMenu, true)]
        private static bool ToggleTouchControlsValidate()
        {
            Menu.SetChecked(TouchMenu, PlayerPrefs.GetInt(TouchPrefKey, 0) == 1);
            return true;
        }

        public static bool ModeRequested => PlayerPrefs.GetInt(ModePrefKey, 0) == 1;

        [MenuItem("Lucille's Journey/AR Tabletop/Remove AR (turn the AR scripts off again)")]
        public static void RemoveAr()
        {
            SessionState.SetBool(RunningFlag, false);
            foreach (var target in DefineTargets)
            {
                var defines = ReadDefines(target);
                if (defines.Remove(Define)) PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
            }
            Log("AR scripts switched off. Packages were left installed; remove them in the Package Manager if you want.");
        }

        // ------------------------------------------------------------------------------------------------------------
        // Status (used by the window)
        // ------------------------------------------------------------------------------------------------------------

        public static bool PackagesInstalled => Packages.All(IsInstalled);
        public static bool ArTypesLoaded => FindType("UnityEngine.XR.ARFoundation.ARSession") != null && FindType("Unity.XR.CoreUtils.XROrigin") != null;
        public static bool DefineSet => DefineTargets.All(t => ReadDefines(t).Contains(Define));
        public static bool AndroidSettingsOk =>
            (int)PlayerSettings.Android.minSdkVersion >= (int)AndroidSdkVersions.AndroidApiLevel24 &&
            PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP &&
            PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64;
        public static bool IosSettingsOk => !string.IsNullOrEmpty(PlayerSettings.iOS.cameraUsageDescription);

        private static bool IsInstalled(string id) =>
            UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Any(p => p.name == id);

        // ------------------------------------------------------------------------------------------------------------
        // The run
        // ------------------------------------------------------------------------------------------------------------

        private static void Resume()
        {
            deadline = EditorApplication.timeSinceStartup + 240.0;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (request != null && !request.IsCompleted) return;

            EditorApplication.update -= Tick;
            try { Step(); }
            catch (Exception e) { Fail("Setup stopped: " + e.Message); Debug.LogException(e); }
        }

        private static void Step()
        {
            if (request != null)
            {
                bool failed = request.Status == StatusCode.Failure;
                string error = failed ? request.Error.message : null;
                request = null;
                if (failed) { Fail("Package install failed: " + error); return; }
            }

            if (!PackagesInstalled)
            {
                var missing = Packages.Where(p => !IsInstalled(p)).ToArray();
                Log("Installing packages: " + string.Join(", ", missing) + " (this can take a minute)...");
                request = Client.AddAndRemove(missing);
                Resume();
                return;
            }

            if (!ArTypesLoaded)
            {
                if (EditorApplication.timeSinceStartup > deadline) { Fail("The AR packages did not finish importing. Try again in a moment."); return; }
                if (!waitingLogged) { Log("Waiting for the AR packages to finish importing..."); waitingLogged = true; }
                EditorApplication.update += Tick; // checked again next editor tick
                return;
            }

            waitingLogged = false;
            if (!DefineSet)
            {
                SetDefine();
                Log("Enabled the AR scripts (" + Define + "). Unity will recompile and setup continues automatically.");
                return; // recompile -> domain reload -> static constructor resumes
            }

            ApplyProjectSettings();
            var notes = new List<string>();
            bool loadersOk = EnableLoaders(notes);
            string urp = AddUrpBackgroundFeature();
            notes.Add(urp);
            AssetDatabase.SaveAssets();

            SessionState.SetBool(RunningFlag, false);
            Finish(loadersOk, notes);
        }

        private static void Fail(string message)
        {
            SessionState.SetBool(RunningFlag, false);
            request = null;
            Debug.LogError("[AR setup] " + message);
            EditorUtility.DisplayDialog("AR setup", message, "OK");
        }

        private static void Finish(bool loadersOk, List<string> notes)
        {
            string summary = "AR tabletop setup finished.\n\n" + string.Join("\n", notes.Select(n => "- " + n));
            if (!loadersOk)
            {
                summary += "\n\nOne step left: install Android Build Support in Unity Hub if it is missing, switch the build platform to Android, " +
                           "then in Project Settings > XR Plug-in Management tick Google ARCore on the Android tab. " +
                           "That window will open now - then run this tool again to confirm.";
            }
            summary += "\n\nNo scene changes are needed. Build to a phone and use \"Play in AR\" on the main menu.";
            Log(summary);
            EditorUtility.DisplayDialog("AR setup", summary, "OK");
            if (!loadersOk) SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }

        // ------------------------------------------------------------------------------------------------------------
        // Step 2 - scripting define
        // ------------------------------------------------------------------------------------------------------------

        private static List<string> ReadDefines(NamedBuildTarget target) =>
            PlayerSettings.GetScriptingDefineSymbols(target).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        public static void SetDefine()
        {
            foreach (var target in DefineTargets)
            {
                var defines = ReadDefines(target);
                if (defines.Contains(Define)) continue;
                defines.Add(Define);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
            }
        }

        // ------------------------------------------------------------------------------------------------------------
        // Step 3 - player settings
        // ------------------------------------------------------------------------------------------------------------

        public static void ApplyProjectSettings()
        {
            if ((int)PlayerSettings.Android.minSdkVersion < (int)AndroidSdkVersions.AndroidApiLevel24)
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

            if (string.IsNullOrEmpty(PlayerSettings.iOS.cameraUsageDescription))
                PlayerSettings.iOS.cameraUsageDescription = "The camera is used to show Lucille's Journey on your table in augmented reality.";

            TrySetArCoreOptional();
            Log("Applied Android (API 24+, IL2CPP, ARM64, OpenGLES3) and iOS (camera usage text) settings.");
        }

        // "Optional" lets phones without ARCore still install the game and play it in flat mode.
        private static void TrySetArCoreOptional()
        {
            try
            {
                var type = FindType("UnityEditor.XR.ARCore.ARCoreSettings") ?? FindType("UnityEngine.XR.ARCore.ARCoreSettings");
                var get = type?.GetMethod("GetOrCreateSettings", BindingFlags.Public | BindingFlags.Static);
                var settings = get?.Invoke(null, null);
                var field = type?.GetProperty("requirement") ?? (MemberInfo)type?.GetField("requirement");
                if (settings == null || field == null) return;

                Type enumType = field is PropertyInfo p ? p.PropertyType : ((FieldInfo)field).FieldType;
                object optional = Enum.Parse(enumType, "Optional");
                if (field is PropertyInfo pi) pi.SetValue(settings, optional); else ((FieldInfo)field).SetValue(settings, optional);
                EditorUtility.SetDirty((UnityEngine.Object)settings);
                Log("ARCore requirement set to Optional, so non-AR phones can still play in normal mode.");
            }
            catch (Exception e) { Debug.Log("[AR setup] Could not set ARCore to Optional (do it in XR Plug-in Management > ARCore if you want): " + e.Message); }
        }

        // ------------------------------------------------------------------------------------------------------------
        // Step 4 - XR loaders (by name, so no compile-time dependency)
        // ------------------------------------------------------------------------------------------------------------

        public static bool AndroidModuleInstalled => BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android);
        public static bool IosModuleInstalled => BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS);

        public static bool EnableLoaders(List<string> notes = null)
        {
            bool androidOk = true, iosOk = true;
            if (AndroidModuleInstalled)
            {
                androidOk = EnableLoader(BuildTargetGroup.Android, "UnityEngine.XR.ARCore.ARCoreLoader");
                notes?.Add(androidOk ? "ARCore loader enabled (Android)." : "ARCore loader NOT enabled yet (Android).");
            }
            else notes?.Add("Android Build Support is not installed - install it in Unity Hub (Installs > your version > Add modules), then run this tool again.");

            if (IosModuleInstalled)
            {
                iosOk = EnableLoader(BuildTargetGroup.iOS, "UnityEngine.XR.ARKit.ARKitLoader");
                notes?.Add(iosOk ? "ARKit loader enabled (iOS)." : "ARKit loader NOT enabled yet (iOS).");
            }
            else notes?.Add("iOS Build Support not installed - skipped (only needed to build for iPhone, which requires a Mac).");

            return AndroidModuleInstalled && androidOk && iosOk;
        }

        /// <summary>Optional: lets the editor run AR in a simulated room. Affects the PC loader list, so it is not done automatically.</summary>
        public static bool EnableSimulationLoader() =>
            EnableLoader(BuildTargetGroup.Standalone, "UnityEngine.XR.Simulation.SimulationLoader");

        private static bool EnableLoader(BuildTargetGroup group, string loaderTypeName)
        {
            try
            {
                var perBuild = FindType("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget");
                var store = FindType("UnityEditor.XR.Management.Metadata.XRPackageMetadataStore");
                if (perBuild == null || store == null) return false;

                object general = perBuild.GetMethod("XRGeneralSettingsForBuildTarget", BindingFlags.Public | BindingFlags.Static)
                    ?.Invoke(null, new object[] { group });
                if (general == null) return false; // settings assets do not exist until the XR window has been opened once

                object manager = general.GetType().GetProperty("AssignedSettings")?.GetValue(general);
                if (manager == null) return false;

                var assign = store.GetMethod("AssignLoader", BindingFlags.Public | BindingFlags.Static);
                return assign != null && (bool)assign.Invoke(null, new[] { manager, loaderTypeName, (object)group });
            }
            catch (Exception e)
            {
                Debug.Log("[AR setup] Could not enable " + loaderTypeName + " automatically: " + e.Message);
                return false;
            }
        }

        // ------------------------------------------------------------------------------------------------------------
        // Step 5 - URP: AR camera background renderer feature
        // ------------------------------------------------------------------------------------------------------------

        public static string AddUrpBackgroundFeature()
        {
            var assets = new HashSet<RenderPipelineAsset>();
            if (GraphicsSettings.defaultRenderPipeline != null) assets.Add(GraphicsSettings.defaultRenderPipeline);
            if (GraphicsSettings.currentRenderPipeline != null) assets.Add(GraphicsSettings.currentRenderPipeline);
            if (QualitySettings.renderPipeline != null) assets.Add(QualitySettings.renderPipeline);
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                var q = QualitySettings.GetRenderPipelineAssetAt(i);
                if (q != null) assets.Add(q);
            }
            if (assets.Count == 0) return "Built-in render pipeline: no camera-background setup needed.";

            Type featureType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "ARBackgroundRendererFeature");
            if (featureType == null) return "URP found, but AR Foundation's background feature type was not found - add 'AR Background Renderer Feature' to your renderer by hand.";

            int added = 0, already = 0;
            bool uses2D = false;
            foreach (var asset in assets)
            {
                var so = new SerializedObject(asset);
                var list = so.FindProperty("m_RendererDataList");
                if (list == null) continue;
                for (int i = 0; i < list.arraySize; i++)
                {
                    var data = list.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
                    if (data == null) continue;
                    if (data.GetType().Name.Contains("2D")) uses2D = true;
                    if (AddFeature(data, featureType)) added++; else already++;
                }
            }

            string result = added > 0 ? $"Added the AR camera-background feature to {added} URP renderer(s)." : "AR camera-background feature already present on the URP renderer(s).";
            if (uses2D)
                result += " Note: your project uses the URP 2D Renderer. If the camera feed shows black on a phone, create a Universal Renderer, add the AR feature to it and assign it to the AR camera.";
            return result;
        }

        private static bool AddFeature(ScriptableObject rendererData, Type featureType)
        {
            var so = new SerializedObject(rendererData);
            var features = so.FindProperty("m_RendererFeatures");
            var map = so.FindProperty("m_RendererFeatureMap");
            if (features == null || map == null) return false;

            for (int i = 0; i < features.arraySize; i++)
            {
                var existing = features.GetArrayElementAtIndex(i).objectReferenceValue;
                if (existing != null && existing.GetType() == featureType) return false;
            }

            var feature = ScriptableObject.CreateInstance(featureType);
            feature.name = "ARBackgroundRendererFeature";
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId)) return false;

            so.Update();
            features.arraySize++;
            features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = feature;
            map.arraySize++;
            map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(rendererData);
            return true;
        }

        // ------------------------------------------------------------------------------------------------------------

        private static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }

        private static void Log(string message) => Debug.Log("[AR setup] " + message);
    }

    /// <summary>Checklist window: shows what is done and lets you run any step on its own.</summary>
    public class ARSetupWindow : EditorWindow
    {
        private void OnInspectorUpdate() => Repaint();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("AR Tabletop", EditorStyles.boldLabel);
            Row("AR packages installed (AR Foundation, ARCore, ARKit)", ARTabletopSetup.PackagesInstalled);
            Row("AR scripts enabled (" + ARTabletopSetup.Define + " defined)", ARTabletopSetup.DefineSet);
            Row("Android settings (API 24+, IL2CPP, ARM64)", ARTabletopSetup.AndroidSettingsOk);
            Row("iOS camera usage text", ARTabletopSetup.IosSettingsOk);

            EditorGUILayout.Space();
            if (GUILayout.Button("Set Up Everything", GUILayout.Height(32))) ARTabletopSetup.RunEverything();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Individual steps", EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(!ARTabletopSetup.ArTypesLoaded))
            {
                if (GUILayout.Button("Apply project settings")) ARTabletopSetup.ApplyProjectSettings();
                if (GUILayout.Button("Enable ARCore / ARKit loaders")) { if (!ARTabletopSetup.EnableLoaders()) SettingsService.OpenProjectSettings("Project/XR Plug-in Management"); }
                if (GUILayout.Button("Add AR camera feature to URP")) Debug.Log("[AR setup] " + ARTabletopSetup.AddUrpBackgroundFeature());
            }
            if (GUILayout.Button("Enable XR Simulation loader (editor AR testing, optional)")) Debug.Log("[AR setup] XR Simulation loader " + (ARTabletopSetup.EnableSimulationLoader() ? "enabled." : "could not be enabled automatically - tick it under the PC tab."));
            if (GUILayout.Button("Open XR Plug-in Management")) SettingsService.OpenProjectSettings("Project/XR Plug-in Management");

            EditorGUILayout.Space();
            bool request = EditorGUILayout.ToggleLeft("Start game in AR mode (editor testing)", ARTabletopSetup.ModeRequested);
            if (request != ARTabletopSetup.ModeRequested) ARTabletopSetup.ToggleRequest();
            EditorGUILayout.HelpBox(
                "No scene changes are needed: the AR rig is built in code when a gameplay scene starts in AR mode. " +
                "Players choose it with \"Play in AR\" on the main menu. On a phone without AR support the game falls back to normal mode.",
                MessageType.Info);
        }

        private static void Row(string label, bool ok) =>
            EditorGUILayout.LabelField((ok ? "✔  " : "✖  ") + label, ok ? EditorStyles.label : EditorStyles.boldLabel);
    }
}
