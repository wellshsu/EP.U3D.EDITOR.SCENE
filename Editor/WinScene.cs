//---------------------------------------------------------------------//
//                    GNU GENERAL PUBLIC LICENSE                       //
//                       Version 2, June 1991                          //
//                                                                     //
// Copyright (C) Wells Hsu, wellshsu@outlook.com, All rights reserved. //
// Everyone is permitted to copy and distribute verbatim copies        //
// of this license document, but changing it is not allowed.           //
//                  SEE LICENSE.md FOR MORE DETAILS.                   //
//---------------------------------------------------------------------//
using EP.U3D.EDITOR.BASE;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EP.U3D.EDITOR.SCENE
{
    public class WinScene : EditorWindow
    {
        [MenuItem(Constants.MENU_WIN_SCENE, false, 4)]
        public static void Invoke()
        {
            GetWindowWithRect(WindowType, WindowRect, WindowUtility, WindowTitle);
        }
        private static string LATEST_SEARCH_SCENE = Path.GetFullPath("./") + "LATEST_SEARCH_SCENE";
        public static Type WindowType = typeof(WinScene);
        public static Rect WindowRect = new Rect(30, 30, 285, 450);
        public static bool WindowUtility = false;
        public static string WindowTitle = "Scene Ease";
        public static WinScene Instance;
        protected Vector2 scroll = Vector2.zero;
        protected string workspace;
        protected readonly List<string> paths = new List<string>();
        protected readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
        protected static string searchStr = "";
        protected float unsearchHeigth;

        protected virtual void OnEnable()
        {
            workspace = Constants.BUNDLE_SCENE_WORKSPACE;
            Instance = this;
            searchStr = EditorPrefs.GetString(LATEST_SEARCH_SCENE);
            Refresh();
            Selection.selectionChanged += OnSelectChange;
        }

        protected virtual void OnDisable()
        {
            Selection.selectionChanged -= OnSelectChange;
        }

        protected virtual void OnSelectChange()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var index = paths.IndexOf(path);
            if (index > 0)
            {
                if (!string.IsNullOrEmpty(searchStr))
                {
                    unsearchHeigth = index * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            Instance = null;
            paths.Clear();
            assets.Clear();
        }

        protected virtual void OnGUI()
        {
            if (paths.Count > 0 && assets.Count > 0)
            {
                Helper.BeginContents();
                var lSearchStr = searchStr;
                searchStr = Helper.SearchField(searchStr, GUILayout.Height(20));
                if (lSearchStr != searchStr) EditorPrefs.SetString(LATEST_SEARCH_SCENE, searchStr);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Item", GUILayout.Width(200));
                GUILayout.Label("Operate");
                GUILayout.EndHorizontal();
                Helper.EndContents();

                if (string.IsNullOrEmpty(searchStr))
                {
                    scroll.y = unsearchHeigth;
                }
                scroll = GUILayout.BeginScrollView(scroll);
                if (string.IsNullOrEmpty(searchStr))
                {
                    unsearchHeigth = scroll.y;
                }
                for (int i = 0; i < paths.Count; i++)
                {
                    if (assets[i] == null || assets[i].name.IndexOf(searchStr, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(assets[i], typeof(UnityEngine.Object), false, GUILayout.Width(170));
                    if (GUILayout.Button("Edit", GUILayout.Width(50)))
                    {
                        HandleEdit(paths[i]);
                        OnSelectChange();
                    }
                    if (GUILayout.Button("Path", GUILayout.Width(50))) Helper.ShowInExplorer(Path.GetFullPath(paths[i]));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            if (GUILayout.Button("Refresh")) Refresh();
        }

        protected virtual void Refresh()
        {
            paths.Clear();
            assets.Clear();
            Helper.CollectAssets(workspace, paths, ".cs", ".js", ".meta", ".DS_Store");
            for (int i = 0; i < paths.Count; i++)
            {
                string file = paths[i];
                bool valid = false;
                if (file.EndsWith(".unity"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
                    if (asset)
                    {
                        assets.Add(asset);
                        valid = true;
                    }
                }
                if (!valid)
                {
                    paths.RemoveAt(i);
                    i--;
                }
            }
        }

        protected virtual void HandleEdit(string path)
        {
            if (File.Exists(path)) EditorSceneManager.OpenScene(path);
        }
    }
}
