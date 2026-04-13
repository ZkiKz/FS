#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SpriteCutterWindow : EditorWindow
{
    private Texture2D sourceTexture;
    private string outputFolder = "Assets/Sprites";
    private int padding = 1;
    private List<Rect> detectedRegions = new List<Rect>();
    private Color backgroundColor = new Color(0, 0, 0, 0);
    private float colorTolerance = 0.1f;
    private float alphaThreshold = 0.1f;
    private Sprite[] loadedSprites;

    [MenuItem("Tools/子图切割工具")]
    public static void ShowWindow()
    {
        GetWindow<SpriteCutterWindow>("子图切割");
    }

    void OnGUI()
    {
        GUILayout.Label("=== 子图切割工具 ===", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("源图片", sourceTexture, typeof(Texture2D), false);

        EditorGUILayout.Space();

        outputFolder = EditorGUILayout.TextField("输出文件夹", outputFolder);
        if (GUILayout.Button("浏览..."))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择输出文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                outputFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
            }
        }

        EditorGUILayout.Space();

        padding = EditorGUILayout.IntField("边距 (Padding)", padding);
        padding = Mathf.Max(0, padding);

        EditorGUILayout.Space();

        backgroundColor = EditorGUILayout.ColorField("透明色", backgroundColor);
        GUILayout.Label("（通常保持默认透明色即可）", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        colorTolerance = EditorGUILayout.Slider("颜色容差", colorTolerance, 0f, 1f);
        GUILayout.Label("（颜色相似度阈值，越低越严格）", EditorStyles.miniLabel);

        alphaThreshold = EditorGUILayout.Slider("透明阈值", alphaThreshold, 0f, 1f);
        GUILayout.Label("（Alpha低于此值视为透明）", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        if (sourceTexture == null)
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button("检测子图区域"))
        {
            DetectSubSprites();
        }

        if (GUILayout.Button("切割并导出"))
        {
            CutAndExport();
        }

        GUI.enabled = true;

        EditorGUILayout.Space();

        GUILayout.Label($"检测到 {detectedRegions.Count} 个子图区域", EditorStyles.boldLabel);

        if (detectedRegions.Count > 0)
        {
            EditorGUILayout.Space();
            GUILayout.Label("预览：", EditorStyles.boldLabel);

            Rect previewRect = GUILayoutUtility.GetRect(500, 300);
            DrawPreview(previewRect);
        }
    }

    private void DetectSubSprites()
    {
        if (sourceTexture == null) return;

        detectedRegions.Clear();

        Texture2D readableTexture = GetReadableTexture(sourceTexture);
        if (readableTexture == null)
        {
            EditorUtility.DisplayDialog("错误", "无法读取图片像素数据，请确保图片是可读的（Texture Type = Sprite或Advanced，Read/Write Enabled = true）", "确定");
            return;
        }

        bool[,] visited = new bool[readableTexture.width, readableTexture.height];
        Color[] pixels = readableTexture.GetPixels();

        for (int y = 0; y < readableTexture.height; y++)
        {
            for (int x = 0; x < readableTexture.width; x++)
            {
                Color pixelColor = pixels[y * readableTexture.width + x];
                if (!visited[x, y] && !IsTransparent(pixelColor))
                {
                    Rect bounds = FloodFillBounds(readableTexture, visited, x, y);
                    if (bounds.width > 1 && bounds.height > 1)
                    {
                        detectedRegions.Add(bounds);
                    }
                }
            }
        }

        DestroyImmediate(readableTexture);

        EditorUtility.DisplayDialog("检测完成", $"在图片中检测到 {detectedRegions.Count} 个子图区域", "确定");
    }

    private bool IsTransparent(Color color)
    {
        if (color.a <= alphaThreshold)
            return true;

        float colorDiff = Mathf.Abs(color.r - backgroundColor.r) +
                         Mathf.Abs(color.g - backgroundColor.g) +
                         Mathf.Abs(color.b - backgroundColor.b);
        colorDiff /= 3f;

        return colorDiff <= colorTolerance;
    }

    private Rect FloodFillBounds(Texture2D texture, bool[,] visited, int startX, int startY)
    {
        int width = texture.width;
        int height = texture.height;
        Color[] pixels = texture.GetPixels();

        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startY * width + startX);

        int minX = startX, maxX = startX;
        int minY = startY, maxY = startY;

        while (queue.Count > 0)
        {
            int pos = queue.Dequeue();
            int x = pos % width;
            int y = pos / width;

            if (x < 0 || x >= width || y < 0 || y >= height) continue;
            if (visited[x, y]) continue;

            Color pixelColor = pixels[y * width + x];
            if (IsTransparent(pixelColor)) continue;

            visited[x, y] = true;

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);

            queue.Enqueue(y * width + (x + 1));
            queue.Enqueue(y * width + (x - 1));
            queue.Enqueue((y + 1) * width + x);
            queue.Enqueue((y - 1) * width + x);
        }

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private void CutAndExport()
    {
        if (sourceTexture == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择源图片", "确定");
            return;
        }

        if (detectedRegions.Count == 0)
        {
            DetectSubSprites();
            if (detectedRegions.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "未检测到任何子图区域", "确定");
                return;
            }
        }

        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string[] pathParts = outputFolder.Split('/');
            string currentPath = pathParts[0];
            for (int i = 1; i < pathParts.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + pathParts[i]))
                {
                    AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                }
                currentPath += "/" + pathParts[i];
            }
        }

        Texture2D readableTexture = GetReadableTexture(sourceTexture);
        if (readableTexture == null)
        {
            EditorUtility.DisplayDialog("错误", "无法读取图片像素数据", "确定");
            return;
        }

        int count = 0;
        int paddedWidth = 0;
        int paddedHeight = 0;

        foreach (Rect region in detectedRegions)
        {
            count++;

            paddedWidth = Mathf.Min(readableTexture.width - (int)region.x, (int)region.width + padding * 2);
            paddedHeight = Mathf.Min(readableTexture.height - (int)region.y, (int)region.height + padding * 2);

            int startX = Mathf.Max(0, (int)region.x - padding);
            int startY = Mathf.Max(0, (int)region.y - padding);

            Color[] regionPixels = readableTexture.GetPixels(startX, startY, paddedWidth, paddedHeight);
            Texture2D subTexture = new Texture2D(paddedWidth, paddedHeight, TextureFormat.RGBA32, false);
            subTexture.SetPixels(regionPixels);
            subTexture.Apply();

            byte[] pngData = subTexture.EncodeToPNG();
            string spriteName = $"{sourceTexture.name}_sprite_{count}";
            string pngPath = $"{outputFolder}/{spriteName}.png";

            File.WriteAllBytes(Application.dataPath + "/" + pngPath.Substring("Assets/".Length), pngData);
            DestroyImmediate(subTexture);
        }

        DestroyImmediate(readableTexture);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "导出完成",
            $"已成功切割并导出 {count} 个子图！\n\n输出目录: {outputFolder}",
            "确定"
        );
    }

    private Texture2D GetReadableTexture(Texture2D texture)
    {
        if (texture.isReadable) return texture;

        string path = AssetDatabase.GetAssetPath(texture);
        if (!string.IsNullOrEmpty(path))
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();

                Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (newTexture != null && newTexture.isReadable)
                {
                    return newTexture;
                }
            }
        }

        RenderTexture renderTexture = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.ARGB32
        );

        Graphics.Blit(texture, renderTexture);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return readableTexture;
    }

    private void DrawPreview(Rect previewRect)
    {
        if (sourceTexture == null) return;

        float textureWidth = sourceTexture.width;
        float textureHeight = sourceTexture.height;
        float aspectRatio = textureWidth / textureHeight;

        float previewWidth = previewRect.width - 20;
        float previewHeight = previewRect.height - 20;

        float displayWidth, displayHeight;
        if (aspectRatio > previewWidth / previewHeight)
        {
            displayWidth = previewWidth;
            displayHeight = previewWidth / aspectRatio;
        }
        else
        {
            displayHeight = previewHeight;
            displayWidth = previewHeight * aspectRatio;
        }

        Rect imageRect = new Rect(
            previewRect.x + (previewWidth - displayWidth) / 2,
            previewRect.y + (previewHeight - displayHeight) / 2,
            displayWidth,
            displayHeight
        );

        GUI.DrawTexture(imageRect, sourceTexture);

        if (detectedRegions.Count > 0)
        {
            float scaleX = displayWidth / textureWidth;
            float scaleY = displayHeight / textureHeight;

            Color outlineColor = Color.red;
            outlineColor.a = 0.8f;

            foreach (Rect region in detectedRegions)
            {
                Rect highlightRect = new Rect(
                    imageRect.x + region.x * scaleX,
                    imageRect.y + (textureHeight - region.y - region.height) * scaleY,
                    region.width * scaleX,
                    region.height * scaleY
                );

                DrawOutline(highlightRect, outlineColor, 2f);
            }
        }

        GUILayout.Space(previewRect.height + 10);
    }

    private void DrawOutline(Rect rect, Color color, float width)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, width), tex);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - width, rect.width, width), tex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, width, rect.height), tex);
        GUI.DrawTexture(new Rect(rect.xMax - width, rect.y, width, rect.height), tex);

        DestroyImmediate(tex);
    }
}
#endif
