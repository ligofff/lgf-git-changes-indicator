using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GitUnityIntegration
{
    private static readonly Dictionary<string, GUIStyle> _styles = new Dictionary<string, GUIStyle>();
    private static readonly Dictionary<Color, Texture2D> _textures = new Dictionary<Color, Texture2D>();
    
    static GitUnityIntegration()
    {
        EditorApplication.projectChanged += RetrieveChanges;
        EditorApplication.projectWindowItemOnGUI += DrawGitStatus;
        RetrieveChanges();
    }

    [MenuItem("Git/Retrieve Changes")]
    public static void RetrieveChanges()
    {
        Task.Run(() =>
        {
            string output = RunGitCommand("diff --name-status HEAD");
            GitStatusCache.UpdateCache(output);
            EditorApplication.QueuePlayerLoopUpdate();
        });
    }

    private static string RunGitCommand(string args)
    {
        using (Process process = new Process())
        {
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return output;
        }
    }
    
    private static void DrawGitStatus(string guid, Rect selectionRect)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (!GitStatusCache.FileStatuses.TryGetValue(path, out string status))
        {
            return;
        }

        Color color;
        switch (status)
        {
            case "M":
                color = new Color(1f, 0.8f, 0f, 0.77f);
                break;
            case "A":
                color = new Color(0f, 1f, 0f, 0.77f);
                break;
            case "D":
                color = new Color(1f, 0f, 0f, 0.77f);
                break;
            default:
                color = new Color(1f, 1f, 1f, 0.77f);
                break;
        }

        if (!_styles.TryGetValue(status, out GUIStyle style))
        {
            style = new GUIStyle(EditorStyles.label)
            {
                normal = new GUIStyleState()
                {
                    textColor = Color.black, background = MakeTexture(2, 2, color)
                }
            };
            _styles.Add(status, style);
        }

        GUI.Box(new Rect(selectionRect.x, selectionRect.y, 15, 15), status, style);
    }

    private static Texture2D MakeTexture(int width, int height, Color col)
    {
        if (!_textures.TryGetValue(col, out Texture2D result))
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            _textures.Add(col, result);
        }

        return result;
    }
}