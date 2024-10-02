// Created by LunarEclipse on 2024-10-02 16:10.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Luna.Internal
{
    public class MenuUtils
    {
        public static void AddMenuItem(string name, int priority, Action execute, Func<bool> validate = null, string shortcut = "", bool @checked = false)
        {
            UnityEditor.Menu.AddMenuItem(name, shortcut, @checked, priority, execute, validate);
        }
        
        public static void RemoveMenuItem(string name)
        {
            UnityEditor.Menu.RemoveMenuItem(name);
        }
        
        public static List<MenuItem> GetMenuItems(string menuPath, bool includeSeparators = false, bool localized = false)
        {
            var scriptingMenuItems = UnityEditor.Menu.GetMenuItems(menuPath, includeSeparators, localized);
            var menuItems = scriptingMenuItems.Select(scriptingMenuItem => new MenuItem { scriptingMenuItem = scriptingMenuItem }).ToList();
            return menuItems;
        }
    }

    public class MenuItem
    {
        internal ScriptingMenuItem scriptingMenuItem;
        
        public string Path => this.scriptingMenuItem.path;

        public bool IsSeparator => this.scriptingMenuItem.isSeparator;

        public int Priority => this.scriptingMenuItem.priority;
    }
}