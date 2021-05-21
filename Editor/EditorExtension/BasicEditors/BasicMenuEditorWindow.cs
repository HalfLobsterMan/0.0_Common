﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CZToolKit.Core.Editors
{
    [Serializable]
    public abstract class BasicMenuEditorWindow : BasicEditorWindow
    {
        [SerializeField]
        ResizableArea resizableArea = new ResizableArea();
        protected Rect resizableAreaRect = new Rect(0, 0, 150, 150);

        string searchText;
        SearchField searchField;
        CZMenuTreeView menuTreeView;
        TreeViewState treeViewState = new TreeViewState();

        protected virtual float LeftMinWidth { get { return 50; } }
        protected virtual float RightMinWidth { get { return 500; } }

        protected virtual void OnEnable()
        {
            resizableArea.minSize = new Vector2(LeftMinWidth, 50);
            resizableArea.side = 10;
            resizableArea.EnableSide(UIDirection.Right);
            resizableArea.SideOffset[UIDirection.Right] = resizableArea.side / 2;

            searchField = new SearchField();
            menuTreeView = BuildMenuTree(treeViewState);
            menuTreeView.Reload();
        }

        void OnGUI()
        {
            resizableArea.maxSize = position.size;

            resizableAreaRect.height = position.height;
            resizableAreaRect = resizableArea.OnGUI(resizableAreaRect);

            Rect searchFieldRect = resizableAreaRect;
            searchFieldRect.height = 20;
            searchFieldRect.y += 3;
            searchFieldRect.x += 5;
            searchFieldRect.width -= 10;
            string tempSearchText = searchField.OnGUI(searchFieldRect, searchText);
            if (tempSearchText != searchText)
            {
                searchText = tempSearchText;
                menuTreeView.searchString = searchText;
            }

            Rect treeviewRect = resizableAreaRect;
            treeviewRect.y += 20;
            treeviewRect.height -= 20;
            menuTreeView.OnGUI(treeviewRect);

            Rect sideRect = resizableAreaRect;
            sideRect.x += sideRect.width;
            sideRect.width = 1;
            EditorGUI.DrawRect(sideRect, new Color(0.5f, 0.5f, 0.5f, 1));

            Rect rightRect = sideRect;
            rightRect.x += rightRect.width + 1;
            rightRect.width = position.width - resizableAreaRect.width - 3;
            rightRect.width = Mathf.Max(rightRect.width, RightMinWidth);

            GUILayout.BeginArea(rightRect);
            rightRect.x = 0;
            rightRect.y = 0;
            IList<int> selection = menuTreeView.GetSelection();
            if (selection.Count > 0)
            {
                OnRightGUI(rightRect, menuTreeView.Find(selection[0]) as CZMenuTreeViewItem);
            }
            GUILayout.EndArea();
        }

        protected abstract CZMenuTreeView BuildMenuTree(TreeViewState _treeViewState);

        protected virtual void OnRightGUI(Rect _rect, CZMenuTreeViewItem _selectedItem) { }
    }

    public class CZMenuTreeView : CZTreeView
    {
        public CZMenuTreeView(TreeViewState state) : base(state)
        {
            rowHeight = 30;
#if !UNITY_2019_1_OR_NEWER
            customFoldoutYOffset = rowHeight / 2 - 8;
#endif
        }

        public T AddMenuItem<T>(string _path) where T : CZMenuTreeViewItem, new()
        {
            return AddMenuItem<T>(_path, (Texture2D)null);
        }

        public T AddMenuItem<T>(string _path, Texture2D _icon) where T : CZMenuTreeViewItem, new()
        {
            if (string.IsNullOrEmpty(_path)) return null;
            T item = new T();
            item.icon = _icon;
            return item;
        }

        public string GetParentPath(string _path)
        {
            int index = _path.LastIndexOf('/');
            if (index == -1) return null;
            return _path.Substring(0, index);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            Rect lineRect = args.rowRect;
            lineRect.y += lineRect.height;
            lineRect.height = 1;
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 1));

            CZMenuTreeViewItem item = args.item as CZMenuTreeViewItem;

            Rect labelRect = args.rowRect;
            if (hasSearch)
            {
                labelRect.x += depthIndentWidth;
                labelRect.width -= labelRect.x;
            }
            else
            {
                labelRect.x += item.depth * depthIndentWidth + depthIndentWidth;
                labelRect.width -= labelRect.x;
            }
            GUI.Label(labelRect, EditorGUIExtension.GetGUIContent(item.displayName, item.icon), EditorStylesExtension.LeftLabelStyle);
            item.itemDrawer?.Invoke(args.rowRect, item);
        }
    }

    public class CZMenuTreeViewItem : CZTreeViewItem
    {
        public Action<Rect, CZMenuTreeViewItem> itemDrawer;
        public CZMenuTreeViewItem() : base() { }
    }
}