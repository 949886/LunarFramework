// Created by LunarEclipse on 2024-7-13 0:8.

namespace Modules.UI.Misc
{
    using UnityEngine;
    using UnityEngine.UI;

    public static class ColorBlockExtension
    {
        public static ColorBlock ClearColor(this ColorBlock colorBlock)
        {
            colorBlock.normalColor = Color.clear;
            colorBlock.highlightedColor = Color.clear;
            colorBlock.pressedColor = Color.clear;
            colorBlock.selectedColor = Color.clear;
            colorBlock.disabledColor = Color.clear;
            return colorBlock;
        }
        
        public static ColorBlock WithAlpha(this ColorBlock colorBlock, float alpha)
        {
            colorBlock.normalColor = new Color(colorBlock.normalColor.r, colorBlock.normalColor.g, colorBlock.normalColor.b, alpha);
            colorBlock.highlightedColor = new Color(colorBlock.highlightedColor.r, colorBlock.highlightedColor.g, colorBlock.highlightedColor.b, alpha);
            colorBlock.pressedColor = new Color(colorBlock.pressedColor.r, colorBlock.pressedColor.g, colorBlock.pressedColor.b, alpha);
            colorBlock.selectedColor = new Color(colorBlock.selectedColor.r, colorBlock.selectedColor.g, colorBlock.selectedColor.b, alpha);
            colorBlock.disabledColor = new Color(colorBlock.disabledColor.r, colorBlock.disabledColor.g, colorBlock.disabledColor.b, alpha);
            return colorBlock;
        }
    }
}