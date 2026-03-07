using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SpriteSheetExportWindow : EditorWindow
{
    private int cellWidth = 64;
    private int cellHeight = 64;
    private AnimationClip targetClip;

    [MenuItem("Assets/2D/Export Sprite Sheet (Custom Size)", false, 2000)]
public static void OpenWindow()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;
        if (clip == null) return;
        SpriteSheetExportWindow window = GetWindow<SpriteSheetExportWindow>("SpriteSheet 导出");
        window.targetClip = clip;
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("提示：如果导出报错，请尝试调大 Cell 尺寸。\n当前模式：强制时间轴采样 + 边界保护。", MessageType.Info);
        cellWidth = EditorGUILayout.IntField("Width:", cellWidth);
        cellHeight = EditorGUILayout.IntField("Height:", cellHeight);

        if (GUILayout.Button("开始导出", GUILayout.Height(40)))
        {
            ExportLogic(targetClip, cellWidth, cellHeight);
        }
    }

    private static void ExportLogic(AnimationClip clip, int cellW, int cellH)
    {
        // 1. 采样所有帧
        List<Sprite> sprites = new List<Sprite>();
        float frameRate = clip.frameRate;
        // 关键：计算真实总帧数，采样间隔必须极小以防漏掉极短的关键帧
        float duration = clip.length;
        int frameCount = Mathf.RoundToInt(duration * frameRate) + 1;

        EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        if (bindings.Length == 0) return;

        for (int i = 0; i < frameCount; i++)
        {
            float t = i / frameRate;
            Sprite s = GetSpriteAtTime(clip, bindings[0], t);
            if (s != null) sprites.Add(s);
        }

        if (sprites.Count == 0) return;

        // 2. 准备大图
        int columns = 8;
        int rows = Mathf.CeilToInt((float)sprites.Count / columns);
        Texture2D sheet = new Texture2D(cellW * columns, cellH * rows, TextureFormat.RGBA32, false);
        
        // 初始化全透明
        Color[] clearPixels = new Color[sheet.width * sheet.height];
        for (int p = 0; p < clearPixels.Length; p++) clearPixels[p] = Color.clear;
        sheet.SetPixels(clearPixels);

        // 3. 安全绘制逻辑
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite s = sprites[i];
            PrepareTexture(s.texture);

            // 获取源像素
            Rect r = s.textureRect;
            Color[] srcPixels = s.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);

            // 计算格子位置
            int col = i % columns;
            int row = rows - 1 - (i / columns);
            int cellBaseX = col * cellW;
            int cellBaseY = row * cellH;

            // 计算 Pivot 对齐偏移
            int offsetX = (cellW / 2) - Mathf.RoundToInt(s.pivot.x);
            int offsetY = (cellH / 2) - Mathf.RoundToInt(s.pivot.y);

            // --- 核心修复：逐像素安全拷贝，解决 Buffer Bounds 报错 ---
            for (int py = 0; py < (int)r.height; py++)
            {
                for (int px = 0; px < (int)r.width; px++)
                {
                    int targetX = cellBaseX + offsetX + px;
                    int targetY = cellBaseY + offsetY + py;

                    // 只在当前格子的范围内且在大图范围内才绘制
                    if (targetX >= cellBaseX && targetX < cellBaseX + cellW &&
                        targetY >= cellBaseY && targetY < cellBaseY + cellH)
                    {
                        sheet.SetPixel(targetX, targetY, srcPixels[py * (int)r.width + px]);
                    }
                }
            }
        }

        sheet.Apply();
        
        // 4. 保存
        string path = AssetDatabase.GetAssetPath(clip);
        string savePath = Path.Combine(Path.GetDirectoryName(path), clip.name + ".png");
        File.WriteAllBytes(savePath, sheet.EncodeToPNG());
        AssetDatabase.Refresh();
        
        Debug.Log($"导出成功！总计 {sprites.Count} 帧。");
        EditorUtility.DisplayDialog("完成", $"已导出 {sprites.Count} 帧到 {savePath}", "OK");
    }

    private static Sprite GetSpriteAtTime(AnimationClip clip, EditorCurveBinding binding, float time)
    {
        ObjectReferenceKeyframe[] keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        if (keys == null || keys.Length == 0) return null;
        Sprite last = keys[0].value as Sprite;
        foreach (var k in keys)
        {
            if (k.time <= time + 0.001f) last = k.value as Sprite;
            else break;
        }
        return last;
    }

    private static void PrepareTexture(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && (!ti.isReadable || ti.textureCompression != TextureImporterCompression.Uncompressed))
        {
            ti.isReadable = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
    }
}