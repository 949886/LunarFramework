using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AnimationClipToSpriteSheet : MonoBehaviour
{
    [MenuItem("Assets/2D/Export Sprite Sheet from Animation Clip", false, 2000)]
    public static void ExportSheet()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;
        if (clip == null) return;

        List<Sprite> sprites = GetSpritesFromClip(clip);
        if (sprites.Count == 0) return;

        // 1. 找到所有帧中的最大尺寸，确保格子够大
        int maxW = 0;
        int maxH = 0;
        foreach (var s in sprites)
        {
            maxW = Mathf.Max(maxW, Mathf.RoundToInt(s.rect.width));
            maxH = Mathf.Max(maxH, Mathf.RoundToInt(s.rect.height));
        }

        int columns = 8;
        int rows = Mathf.CeilToInt((float)sprites.Count / columns);

        // 2. 创建大图
        Texture2D sheet = new Texture2D(maxW * columns, maxH * rows, TextureFormat.RGBA32, false);
        // 初始化全透明背景
        Color[] clearPixels = new Color[sheet.width * sheet.height];
        for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.clear;
        sheet.SetPixels(clearPixels);

        // 3. 逐帧安全绘制
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite s = sprites[i];
            EnsureTextureReadable(s.texture);

            // 获取该帧实际像素尺寸
            int frameW = Mathf.RoundToInt(s.textureRect.width);
            int frameH = Mathf.RoundToInt(s.textureRect.height);
            Color[] pixels = s.texture.GetPixels((int)s.textureRect.x, (int)s.textureRect.y, frameW, frameH);

            // 计算格子位置 (Unity 左下角为 0,0)
            int col = i % columns;
            int row = rows - 1 - (i / columns); 
            
            int cellStartX = col * maxW;
            int cellStartY = row * maxH;

            // 居中计算：将当前帧放在格子的中心
            int offsetX = (maxW - frameW) / 2;
            int offsetY = (maxH - frameH) / 2;

            // 写入像素
            sheet.SetPixels(cellStartX + offsetX, cellStartY + offsetY, frameW, frameH, pixels);
        }

        sheet.Apply();

        // 4. 保存
        byte[] bytes = sheet.EncodeToPNG();
        string path = AssetDatabase.GetAssetPath(clip);
        string savePath = Path.Combine(Path.GetDirectoryName(path), clip.name + "_Sheet.png");
        File.WriteAllBytes(savePath, bytes);
        
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan>[SpriteSheet]</color> 导出成功：{savePath} (格子大小: {maxW}x{maxH})");
    }

    private static void EnsureTextureReadable(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        if (string.IsNullOrEmpty(path)) return;
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && !ti.isReadable)
        {
            ti.isReadable = true;
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
            foreach (var frame in keyframes)
            {
                if (frame.value is Sprite s)
                {
                    // 这里不建议去重，因为动画可能反复调用同一帧，保持序列完整更好
                    sprites.Add(s);
                }
            }
        }
        return sprites;
    }
}