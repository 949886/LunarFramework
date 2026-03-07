using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AnimationClipToSpriteSheet : MonoBehaviour
{
    [MenuItem("Assets/2D/Export Sprite Sheet from Animation Clip", false, 304)]
    public static void ExportSheet()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;
        if (clip == null) return;

        List<Sprite> sprites = GetSpritesFromClip(clip);
        if (sprites.Count == 0) return;

        // 1. 预扫描计算尺寸（同上个版本）
        float maxLeft = 0, maxRight = 0, maxTop = 0, maxBottom = 0;
        foreach (var s in sprites)
        {
            maxLeft = Mathf.Max(maxLeft, s.pivot.x);
            maxRight = Mathf.Max(maxRight, s.rect.width - s.pivot.x);
            maxBottom = Mathf.Max(maxBottom, s.pivot.y);
            maxTop = Mathf.Max(maxTop, s.rect.height - s.pivot.y);
        }

        int cellW = Mathf.CeilToInt(maxLeft + maxRight);
        int cellH = Mathf.CeilToInt(maxBottom + maxTop);
        int targetPivotX = Mathf.CeilToInt(maxLeft);
        int targetPivotY = Mathf.CeilToInt(maxBottom);

        int columns = 8;
        int rows = Mathf.CeilToInt((float)sprites.Count / columns);

        // 2. 创建大图 (使用 RGBA32 保证最高精度)
        Texture2D sheet = new Texture2D(cellW * columns, cellH * rows, TextureFormat.RGBA32, false);
        Color[] clearPixels = new Color[sheet.width * sheet.height];
        for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.clear;
        sheet.SetPixels(clearPixels);

        // 3. 逐帧绘制
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite s = sprites[i];
            if (s == null || s.texture == null) continue;

            // --- 关键步骤：临时修改导入设置以获取原始像素 ---
            PrepareTexture(s.texture);

            int frameW = Mathf.RoundToInt(s.textureRect.width);
            int frameH = Mathf.RoundToInt(s.textureRect.height);
            
            // 使用 GetPixels 读取时，RGBA32 格式能保证数据原封不动
            Color[] pixels = s.texture.GetPixels((int)s.textureRect.x, (int)s.textureRect.y, frameW, frameH);

            int col = i % columns;
            int row = rows - 1 - (i / columns); 
            int offsetX = targetPivotX - Mathf.RoundToInt(s.pivot.x);
            int offsetY = targetPivotY - Mathf.RoundToInt(s.pivot.y);

            sheet.SetPixels(col * cellW + offsetX, row * cellH + offsetY, frameW, frameH, pixels);
        }

        sheet.Apply();

        // 4. 保存为 PNG
        byte[] bytes = sheet.EncodeToPNG();
        string path = AssetDatabase.GetAssetPath(clip);
        string savePath = Path.Combine(Path.GetDirectoryName(path), clip.name + ".png");
        File.WriteAllBytes(savePath, bytes);
        
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>[SpriteSheet]</color> 导出成功！像素已对齐并保持原画质。");
    }

    /// <summary>
    /// 强制修改图片导入设置：开启 Read/Write，禁用压缩，禁用 Mipmaps
    /// </summary>
    private static void PrepareTexture(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        if (string.IsNullOrEmpty(path)) return;

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        bool needsReimport = false;

        // 必须开启读写
        if (!ti.isReadable) { ti.isReadable = true; needsReimport = true; }
        
        // 关键：如果不是 Uncompressed 格式，强制改为真彩色
        // 这样 GetPixels 拿到的就是没有噪点的数据
        if (ti.textureCompression != TextureImporterCompression.Uncompressed)
        {
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            needsReimport = true;
        }

        // 避免 Mipmap 导致的模糊
        if (ti.mipmapEnabled) { ti.mipmapEnabled = false; needsReimport = true; }

        if (needsReimport)
        {
            ti.SaveAndReimport();
        }
    }

    private static List<Sprite> GetSpritesFromClip(AnimationClip clip)
    {
        List<Sprite> sprites = new List<Sprite>();
        EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var binding in objectBindings)
        {
            ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            if (keyframes == null) continue;
            foreach (var frame in keyframes)
            {
                if (frame.value is Sprite s && s != null)
                    sprites.Add(s);
            }
        }
        return sprites;
    }
}