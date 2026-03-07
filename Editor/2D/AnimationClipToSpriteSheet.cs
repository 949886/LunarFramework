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

        // 1. 预扫描：计算基于 Pivot 对齐所需的格子大小
        // 我们需要找到 Pivot 距离各个边界的最大距离
        float maxLeft = 0;
        float maxRight = 0;
        float maxTop = 0;
        float maxBottom = 0;

        foreach (var s in sprites)
        {
            // s.rect 是 Sprite 在原始贴图上的裁剪区域 (像素)
            // s.pivot 是 Pivot 在 s.rect 坐标系下的位置 (像素, 左下角为 0,0)

            // 计算 Pivot 距离 Sprite 四边的像素距离
            float distLeft = s.pivot.x;
            float distRight = s.rect.width - s.pivot.x;
            float distBottom = s.pivot.y;
            float distTop = s.rect.height - s.pivot.y;

            maxLeft = Mathf.Max(maxLeft, distLeft);
            maxRight = Mathf.Max(maxRight, distRight);
            maxBottom = Mathf.Max(maxBottom, distBottom);
            maxTop = Mathf.Max(maxTop, distTop);
        }

        // 最终格子的尺寸 (像素)
        int cellW = Mathf.CeilToInt(maxLeft + maxRight);
        int cellH = Mathf.CeilToInt(maxBottom + maxTop);

        // 统一的 Pivot 在格子中的相对位置
        int targetPivotX = Mathf.CeilToInt(maxLeft);
        int targetPivotY = Mathf.CeilToInt(maxBottom);

        // 2. 计算大图布局
        int columns = 8;
        int rows = Mathf.CeilToInt((float)sprites.Count / columns);

        Texture2D sheet = new Texture2D(cellW * columns, cellH * rows, TextureFormat.RGBA32, false);
        
        // 初始化透明背景
        Color[] clearPixels = new Color[sheet.width * sheet.height];
        for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.clear;
        sheet.SetPixels(clearPixels);

        // 3. 逐帧安全绘制 (对齐 Pivot)
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite s = sprites[i];
            if (s.texture == null) continue;

            EnsureTextureReadable(s.texture);

            // 获取该帧实际像素尺寸 (可能因 Tight Mesh 而小于格子)
            int frameW = Mathf.RoundToInt(s.textureRect.width);
            int frameH = Mathf.RoundToInt(s.textureRect.height);
            
            // 注意：GetObjectReferenceCurve 拿到的可能是未裁剪的 Sprite，
            // 确保使用 textureRect 来获取实际有像素的区域。
            Color[] pixels = s.texture.GetPixels((int)s.textureRect.x, (int)s.textureRect.y, frameW, frameH);

            // 计算格子在 SpriteSheet 中的左下角起始位置 (Unity 坐标系)
            int col = i % columns;
            int row = rows - 1 - (i / columns); 
            int cellStartX = col * cellW;
            int cellStartY = row * cellH;

            // 核心修复：计算 Pivot 对齐所需的偏移
            // 我们希望该帧的 Pivot (s.pivot) 重合到格子的虚拟 Pivot (targetPivotX, targetPivotY)
            int offsetX = targetPivotX - Mathf.RoundToInt(s.pivot.x);
            int offsetY = targetPivotY - Mathf.RoundToInt(s.pivot.y);

            // 写入像素
            try
            {
                sheet.SetPixels(cellStartX + offsetX, cellStartY + offsetY, frameW, frameH, pixels);
            }
            catch (System.ArgumentException e)
            {
                // 如果仍然报错，通常是因为 s.pivot 的计算和 s.textureRect 的不一致导致的边界溢出。
                // 这是一个兜底，确保脚本不崩溃，并提示信息。
                Debug.LogError($"[SpriteSheet] 帧 {i} 绘制失败，通常由不规则的 Pivot 和裁剪引起。\nError: {e.Message}");
            }
        }

        sheet.Apply();

        // 4. 保存
        byte[] bytes = sheet.EncodeToPNG();
        string path = AssetDatabase.GetAssetPath(clip);
        string savePath = Path.Combine(Path.GetDirectoryName(path), clip.name + "_AlignedSheet.png");
        File.WriteAllBytes(savePath, bytes);
        
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>[SpriteSheet]</color> 导出成功 (Pivot已对齐): {savePath}\n格子大小: {cellW}x{cellH}, 统一Pivot: ({targetPivotX},{targetPivotY})");
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
                    // 动画切片中可能包含引用为空的帧，或者重复的帧，这里需要过滤
                    if(s != null)
                        sprites.Add(s);
                }
            }
        }
        return sprites;
    }
}