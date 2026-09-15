using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Exports incrementally numbered Swift Xcode projects from Tools/Build iOS.
/// </summary>
public static class IOSBuildTool
{
    private const string BuildsDirectoryName = "Builds";

    [MenuItem("Tools/Build iOS")]
    public static void BuildIosProject()
    {
        string[] scenes = GetEnabledScenes();
        string outputPath = CreateNextBuildDirectory();
        string version = IncrementPatchVersion(PlayerSettings.bundleVersion);

        // Unity 6000.6 exposes the experimental Swift/SwiftUI Xcode project type through this setting.
        PlayerSettings.xcodeProjectType = XcodeProjectType.Swift;
        PlayerSettings.bundleVersion = version;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[IOSBuildTool] Swift Xcode project built at '{outputPath}'. Version: {version}.");
            EditorUtility.RevealInFinder(outputPath);
            return;
        }

        Debug.LogError($"[IOSBuildTool] iOS build failed. See the Build Report for details. Output: '{outputPath}'.");
    }

    private static string[] GetEnabledScenes()
    {
        string[] scenes = Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("[IOSBuildTool] No enabled scenes are configured in Build Settings.");
        }

        return scenes;
    }

    private static string CreateNextBuildDirectory()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string buildsRoot = Path.Combine(projectRoot, BuildsDirectoryName);
        Directory.CreateDirectory(buildsRoot);

        int buildNumber = 1;
        string outputPath;
        do
        {
            outputPath = Path.Combine(buildsRoot, $"_build{buildNumber}");
            buildNumber++;
        }
        while (Directory.Exists(outputPath) || File.Exists(outputPath));

        Directory.CreateDirectory(outputPath);
        return outputPath;
    }

    private static string IncrementPatchVersion(string version)
    {
        Version parsedVersion;
        if (!Version.TryParse(version, out parsedVersion))
        {
            Debug.LogWarning($"[IOSBuildTool] Bundle version '{version}' is not numeric. Resetting it to 0.1.1.");
            return "0.1.1";
        }

        int minor = Math.Max(parsedVersion.Minor, 0);
        int patch = Math.Max(parsedVersion.Build, 0) + 1;
        return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", parsedVersion.Major, minor, patch);
    }
}
