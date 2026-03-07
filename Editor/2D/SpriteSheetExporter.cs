using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SpriteSheetExporter : MonoBehaviour
{
    [MenuItem("Assets/2D/Export as Sprite Sheet (8 per row)", priority = 303)]
    private static void ExportSprites()
    {
        // 1. 获取选中的所有 Sprite 资源
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先选中至少一个 Sprite 资源！", "好的");
            return;
        }

        List<Sprite> sprites = new List<Sprite>();
        foreach (var obj in selectedObjects) if (obj is Sprite s) sprites.Add(s);

        // 1. 预处理：确保所有原图都是可读且未压缩的
        foreach (var s in sprites) PrepareTextureForRead(s.texture);

        // 2. 计算 Pivot 对齐后的网格尺寸
        float maxLeft = 0, maxRight = 0, maxUp = 0, maxDown = 0;
        foreach (var s in sprites)
        {
            Vector2 pivot = s.pivot;
            Rect r = s.rect;
            maxLeft = Mathf.Max(maxLeft, pivot.x);
            maxRight = Mathf.Max(maxRight, r.width - pivot.x);
            maxDown = Mathf.Max(maxDown, pivot.y);
            maxUp = Mathf.Max(maxUp, r.height - pivot.y);
        }

        int maxWidth = Mathf.CeilToInt(maxLeft + maxRight);
        int maxHeight = Mathf.CeilToInt(maxUp + maxDown);
        int columns = 8;
        int rows = Mathf.CeilToInt((float)sprites.Count / columns);

        // 3. 创建贴图（使用 RGBA32 保证无损颜色）
        Texture2D spriteSheet = new Texture2D(columns * maxWidth, rows * maxHeight, TextureFormat.RGBA32, false);
        
        // 彻底清理背景为纯透明
        Color[] clearColors = new Color[spriteSheet.width * spriteSheet.height];
        for (int i = 0; i < clearColors.Length; i++) clearColors[i] = new Color(0,0,0,0);
        spriteSheet.SetPixels(clearColors);

        // 4. 写入像素
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite s = sprites[i];
            int sw = Mathf.FloorToInt(s.rect.width);
            int sh = Mathf.FloorToInt(s.rect.height);
            
            Color[] pixels = s.texture.GetPixels(
                Mathf.FloorToInt(s.rect.x), 
                Mathf.FloorToInt(s.rect.y), 
                sw, sh
            );

            int offsetX = Mathf.RoundToInt(maxLeft - s.pivot.x);
            int offsetY = Mathf.RoundToInt(maxDown - s.pivot.y);
            int cellX = (i % columns) * maxWidth;
            int cellY = (rows - 1 - (i / columns)) * maxHeight;

            spriteSheet.SetPixels(cellX + offsetX, cellY + offsetY, sw, sh, pixels);
        }

        spriteSheet.Apply();

        // 5. 保存并自动优化设置
        byte[] bytes = spriteSheet.EncodeToPNG();
        string path = AssetDatabase.GetAssetPath(selectedObjects[0]);
        string savePath = Path.Combine(Path.GetDirectoryName(path), "SpriteSheet.png");
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.Refresh();

        // 自动将导出的图片设为无损
        ApplyImportSettings(savePath);

        float pivotX = maxLeft / maxWidth;
        float pivotY = maxDown / maxHeight;
        
        EditorUtility.DisplayDialog("导出成功", 
            $"网格大小: {maxWidth}x{maxHeight}\n" +
            $"建议 Pivot 比例: X:{pivotX:F3}, Y:{pivotY:F3}\n" +
            "已自动应用无损导入设置。", "OK");
    }

    private static void PrepareTextureForRead(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        bool changed = false;
        if (!importer.isReadable) { importer.isReadable = true; changed = true; }
        // 强制设为 Uncompressed (无压缩) 以避免噪点
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) 
        { 
            importer.textureCompression = TextureImporterCompression.Uncompressed; 
            changed = true; 
        }

        if (changed) importer.SaveAndReimport();
    }

    private static void ApplyImportSettings(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point; // 像素风格不模糊
        importer.textureCompression = TextureImporterCompression.Uncompressed; // 结果不压缩
        importer.alphaIsTransparency = true;
        
        importer.SaveAndReimport();
    }
}