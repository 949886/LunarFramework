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

        // 2. 第一遍遍历：确定统一网格单元的最大尺寸
        int maxWidth = 0;
        int maxHeight = 0;
        foreach (var s in sprites)
        {
            // 使用 CeilToInt 确保完全覆盖浮点尺寸
            maxWidth = Mathf.Max(maxWidth, Mathf.CeilToInt(s.rect.width));
            maxHeight = Mathf.Max(maxHeight, Mathf.CeilToInt(s.rect.height));
        }

        // 3. 计算行列和大图尺寸
        int columns = 8;
        int rows = Mathf.CeilToInt((float)sprites.Count / columns);
        int sheetWidth = columns * maxWidth;
        int sheetHeight = rows * maxHeight;

        // 4. 创建新贴图并初始化透明背景
        Texture2D spriteSheet = new Texture2D(sheetWidth, sheetHeight, TextureFormat.RGBA32, false);
        Color[] clearColors = new Color[sheetWidth * sheetHeight];
        for (int i = 0; i < clearColors.Length; i++) clearColors[i] = Color.clear;
        spriteSheet.SetPixels(clearColors);

        // 用于逐行安全写入的工具缓冲区，减少 SetPixel 调用
        Color[] cellPixelsBuffer = new Color[maxWidth * maxHeight];

        // 5. 逐个写入，实现居中
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite sprite = sprites[i];
            EnsureTextureIsReadable(sprite.texture);

            // 原始 Sprite 的整数尺寸
            int sw = Mathf.FloorToInt(sprite.rect.width);
            int sh = Mathf.FloorToInt(sprite.rect.height);
            int sx = Mathf.FloorToInt(sprite.rect.x);
            int sy = Mathf.FloorToInt(sprite.rect.y);

            // 从原图中提取像素
            Color[] spritePixels = sprite.texture.GetPixels(sx, sy, sw, sh);

            // --- 核心逻辑：计算水平和垂直居中偏移量 ---
            // 差值 = 网格宽 - Sprite宽。偏移量 = 差值 / 2 (向下取整)
            int centerXOffset = (maxWidth - sw) / 2;
            int centerYOffset = (maxHeight - sh) / 2;

            // --- 优化写入：构建一个完整的网格像素块 ---
            // 重置缓冲区为全透明
            System.Array.Copy(cellPixelsBuffer, cellPixelsBuffer, 0); // 或者循环填充 clear
            for (int b = 0; b < cellPixelsBuffer.Length; b++) cellPixelsBuffer[b] = Color.clear;

            // 将 Sprite 像素合并进缓冲区，注意坐标转换
            for (int y = 0; y < sh; y++)
            {
                for (int x = 0; x < sw; x++)
                {
                    // 目标坐标：缓冲区内的坐标 (需要加上居中偏移)
                    int targetX = centerXOffset + x;
                    int targetY = centerYOffset + y;

                    // 边界安全检查
                    if (targetX < maxWidth && targetY < maxHeight)
                    {
                        cellPixelsBuffer[targetY * maxWidth + targetX] = spritePixels[y * sw + x];
                    }
                }
            }

            // 计算大图中的网格起始坐标 (从左上角开始排版)
            int cellXOffset = (i % columns) * maxWidth;
            int cellYOffset = (rows - 1 - (i / columns)) * maxHeight;

            // 一次性安全写入一个完整的网格块，绝不会报错
            spriteSheet.SetPixels(cellXOffset, cellYOffset, maxWidth, maxHeight, cellPixelsBuffer);
        }

        spriteSheet.Apply();

        // 6. 保存PNG
        byte[] bytes = spriteSheet.EncodeToPNG();
        string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedObjects[0]));
        string savePath = Path.Combine(directory, "Centered_SpriteSheet.png");

        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完成", $"Sprite Sheet 已生成，大小不一的 Sprite 已在網格中居中对齐。\n網格大小: {maxWidth}x{maxHeight}\n保存至: {savePath}", "太棒了");
    }

    private static void EnsureTextureIsReadable(Texture2D tex)
    {
        if (tex == null) return;
        string path = AssetDatabase.GetAssetPath(tex);
        if (string.IsNullOrEmpty(path)) return; // 可能是动态生成的

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }
}