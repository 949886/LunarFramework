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

        // 1. 计算每个 Sprite 相对于其 Pivot 的四个方向的跨度
        // 这样可以找到一个足够大的网格，容纳所有 Sprite 的偏移
        float maxLeft = 0, maxRight = 0, maxUp = 0, maxDown = 0;

        foreach (var s in sprites)
        {
            // pivot 是像素单位的偏移（相对于 Sprite 矩形左下角）
            Vector2 pivot = s.pivot; 
            Rect r = s.rect;

            maxLeft = Mathf.Max(maxLeft, pivot.x);
            maxRight = Mathf.Max(maxRight, r.width - pivot.x);
            maxDown = Mathf.Max(maxDown, pivot.y);
            maxUp = Mathf.Max(maxUp, r.height - pivot.y);
        }

        // 2. 确定统一网格单元尺寸（基于 Pivot 对齐后的总宽高）
        int maxWidth = Mathf.CeilToInt(maxLeft + maxRight);
        int maxHeight = Mathf.CeilToInt(maxUp + maxDown);

        int columns = 8;
        int rows = Mathf.CeilToInt((float)sprites.Count / columns);
        int sheetWidth = columns * maxWidth;
        int sheetHeight = rows * maxHeight;

        Texture2D spriteSheet = new Texture2D(sheetWidth, sheetHeight, TextureFormat.RGBA32, false);
        
        // 初始化透明
        Color[] clearColors = new Color[sheetWidth * sheetHeight];
        for (int i = 0; i < clearColors.Length; i++) clearColors[i] = Color.clear;
        spriteSheet.SetPixels(clearColors);

        // 3. 逐个写入，基于 Pivot 对齐
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite sprite = sprites[i];
            EnsureTextureIsReadable(sprite.texture);

            int sw = Mathf.FloorToInt(sprite.rect.width);
            int sh = Mathf.FloorToInt(sprite.rect.height);
            
            // 提取像素
            Color[] spritePixels = sprite.texture.GetPixels(
                Mathf.FloorToInt(sprite.rect.x), 
                Mathf.FloorToInt(sprite.rect.y), 
                sw, sh
            );

            // --- 核心对齐逻辑 ---
            // 统一的 Pivot 在网格内部的相对坐标应该是 (maxLeft, maxDown)
            // 当前 Sprite 的 Pivot 在自己矩形内的相对坐标是 (sprite.pivot.x, sprite.pivot.y)
            int offsetX = Mathf.RoundToInt(maxLeft - sprite.pivot.x);
            int offsetY = Mathf.RoundToInt(maxDown - sprite.pivot.y);

            // 计算该网格在大图中的左下角起点
            int cellXOrigin = (i % columns) * maxWidth;
            int cellYOrigin = (rows - 1 - (i / columns)) * maxHeight;

            // 写入像素（考虑 Pivot 偏移）
            spriteSheet.SetPixels(cellXOrigin + offsetX, cellYOrigin + offsetY, sw, sh, spritePixels);
        }

        spriteSheet.Apply();

        // 4. 保存
        byte[] bytes = spriteSheet.EncodeToPNG();
        string path = AssetDatabase.GetAssetPath(selectedObjects[0]);
        string savePath = Path.Combine(Path.GetDirectoryName(path), "Aligned_SpriteSheet.png");
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功", $"已完成 Pivot 对齐导出！\n网格尺寸: {maxWidth}x{maxHeight}", "OK");
    }

    private static void EnsureTextureIsReadable(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }
}