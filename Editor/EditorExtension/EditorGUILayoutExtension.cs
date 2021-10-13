#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.Core.Editors
{
    public static partial class EditorGUILayoutExtension
    {
        /// <summary>  </summary>
        /// <param name="_key"> 用于存取上下文数据 </param>
        /// <returns> visible or not </returns>
        public static bool BeginFadeGroup(string _key, bool _visible, float _speed = 1)
        {
            if (!GUIHelper.TryGetContextData<AnimFloat>(_key, out var contextData))
                contextData.value = new AnimFloat(_visible ? 1 : 0);
            contextData.value.speed = _speed * (_visible ? 1 : 2);
            contextData.value.target = _visible ? 1 : 0;
            float _t = contextData.value.value;
            if (_visible)
            {
                _t--;
                _t = -(_t * _t * _t * _t - 1);
            }
            else
            {
                _t = _t * _t;
            }

            EditorGUIExtension.BeginAlpha(_t);
            return EditorGUILayout.BeginFadeGroup(_t);
        }

        public static void EndFadeGroup()
        {
            EditorGUILayout.EndFadeGroup();
            EditorGUIExtension.EndAlpha();
        }

        public static Rect BeginBoxGroup()
        {
            return EditorGUILayout.BeginVertical(EditorStylesExtension.RoundedBoxStyle);
        }

        public static void EndBoxGroup()
        {
            EditorGUILayout.EndVertical();
        }

        public static (bool foldout, bool enable) BeginToggleGroup(string _label, bool _foldout, bool _enable)
        {
            BeginBoxGroup();
            Rect rect = GUILayoutUtility.GetRect(50, 25);
            rect = EditorGUI.IndentedRect(rect);
            Rect toggleRect = new Rect(rect.x + 20, rect.y, rect.height, rect.height);

            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0)
            {
                if (toggleRect.Contains(current.mousePosition))
                {
                    _enable = !_enable;
                }
                else if (rect.Contains(current.mousePosition))
                {
                    _foldout = !_foldout;
                }
            }

            switch (current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.Repaint:
                    GUI.Box(rect, string.Empty, GUI.skin.button);

                    Rect t = rect;
                    t.xMin += 5;
                    t.xMax = t.xMin + t.height;
                    EditorGUI.Foldout(t, _foldout, string.Empty);

                    toggleRect.width = rect.width - t.width;
                    EditorGUI.ToggleLeft(toggleRect, _label, _enable);
                    break;
                default:
                    break;
            }

            EditorGUI.BeginDisabledGroup(!_enable);
            EditorGUI.indentLevel++;
            return (_foldout, _enable);
        }

        public static void EndToggleGroup()
        {
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EndBoxGroup();
        }

        public static bool BeginFoldout(string _label, bool _foldout)
        {
            BeginBoxGroup();
            Rect rect = GUILayoutUtility.GetRect(50, 25);
            rect = EditorGUI.IndentedRect(rect);

            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0)
            {
                if (rect.Contains(current.mousePosition))
                {
                    _foldout = !_foldout;
                    current.Use();
                }
            }

            switch (current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.Repaint:
                    GUI.Box(rect, string.Empty, GUI.skin.button);

                    Rect t = rect;
                    t.xMin += 5;
                    t.xMax -= 5;
                    EditorGUI.Foldout(t, _foldout, _label);
                    break;
                default:
                    break;
            }

            EditorGUI.indentLevel++;
            return _foldout;
        }

        public static void EndFoldout()
        {
            EditorGUI.indentLevel--;
            EndBoxGroup();
        }

        public static float ScrollList(SerializedProperty _list, float _scroll, ref bool _foldout, int _count = 10)
        {
            _foldout = EditorGUILayout.Foldout(_foldout, _list.displayName, true);

            if (_foldout)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                int size = EditorGUILayout.DelayedIntField("Count", _list.arraySize);
                EditorGUI.indentLevel--;
                int targetIndex = -1;
                targetIndex = EditorGUILayout.DelayedIntField(targetIndex, GUILayout.Width(40));
                GUILayout.EndHorizontal();

                EditorGUI.indentLevel++;

                if (size != _list.arraySize)
                    _list.arraySize = size;

                GUILayout.BeginHorizontal();
                Rect r = EditorGUILayout.BeginVertical();

                if (_list.arraySize > _count)
                {
                    int startIndex = Mathf.CeilToInt(_list.arraySize * _scroll);
                    startIndex = Mathf.Max(0, startIndex);
                    for (int i = startIndex; i < startIndex + _count; i++)
                    {
                        EditorGUILayout.PropertyField(_list.GetArrayElementAtIndex(i));
                    }
                }
                else
                {
                    for (int i = 0; i < _list.arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(_list.GetArrayElementAtIndex(i));
                    }
                }

                EditorGUILayout.EndVertical();
                if (_list.arraySize > _count)
                {
                    GUILayout.Space(20);
                    if (_list.arraySize > _count)
                    {
                        if (Event.current.type == EventType.ScrollWheel && r.Contains(Event.current.mousePosition))
                        {
                            _scroll += Event.current.delta.y * 0.01f;
                            Event.current.Use();
                        }
                        if (targetIndex != -1)
                        {
                            _scroll = Mathf.Clamp01((float)targetIndex / _list.arraySize);
                        }

                        r.xMin += r.width + 5;
                        r.width = 20;
                        _scroll = GUI.VerticalScrollbar(r, _scroll, (float)_count / _list.arraySize, 0, 1);
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

            }
            return _scroll;
        }

        public static string FilePath(string _label, string _path)
        {
            EditorGUILayout.BeginHorizontal();
            _path = EditorGUILayout.TextField(_label, _path);
            Rect rect = GUILayoutUtility.GetLastRect();
            UnityObject uObj = EditorGUIExtension.DragDropAreaSingle(rect, DragAndDropVisualMode.Copy);
            if (uObj != null && AssetDatabase.IsMainAsset(uObj))
            {
                string p = AssetDatabase.GetAssetPath(uObj);
                if (!AssetDatabase.IsValidFolder(p))
                    _path = p;
            }
            if (GUILayout.Button(EditorGUIUtility.FindTexture("FolderEmpty Icon"), EditorStylesExtension.OnlyIconButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
            {
                _path = EditorUtility.OpenFilePanel("Select File", Application.dataPath, "*");
                if (!string.IsNullOrEmpty(_path))
                    _path = _path.Substring(_path.IndexOf("Assets"));
            }
            EditorGUILayout.EndHorizontal();
            return _path;
        }

        public static void FilePath(string _label, SerializedProperty _path)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_path);
            Rect rect = GUILayoutUtility.GetLastRect();
            UnityObject uObj = EditorGUIExtension.DragDropAreaSingle(rect, DragAndDropVisualMode.Copy);
            if (uObj != null && AssetDatabase.IsMainAsset(uObj))
            {
                string p = AssetDatabase.GetAssetPath(uObj);
                if (!AssetDatabase.IsValidFolder(p))
                    _path.stringValue = p;
            }
            if (GUILayout.Button(EditorGUIUtility.FindTexture("FolderEmpty Icon"), EditorStylesExtension.OnlyIconButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
            {
                string p = EditorUtility.OpenFilePanel("Select File", Application.dataPath, "*");
                if (!string.IsNullOrEmpty(p))
                    _path.stringValue = p.Substring(p.IndexOf("Assets"));
            }
            EditorGUILayout.EndHorizontal();
        }

        public static string FolderPath(string _label, string _folder)
        {
            EditorGUILayout.BeginHorizontal();
            _folder = EditorGUILayout.TextField(_label, _folder);
            Rect rect = GUILayoutUtility.GetLastRect();
            UnityObject uObj = EditorGUIExtension.DragDropAreaSingle(rect, DragAndDropVisualMode.Copy);
            if (uObj != null && AssetDatabase.IsMainAsset(uObj))
            {
                string p = AssetDatabase.GetAssetPath(uObj);
                if (AssetDatabase.IsValidFolder(p))
                    _folder = p;
            }
            if (GUILayout.Button(EditorGUIUtility.FindTexture("FolderEmpty Icon"), EditorStylesExtension.OnlyIconButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
            {
                _folder = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(_folder))
                    _folder = _folder.Substring(_folder.IndexOf("Assets"));
            }
            EditorGUILayout.EndHorizontal();
            return _folder;
        }

        public static void FolderPath(string _label, SerializedProperty _folder)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_folder);
            Rect rect = GUILayoutUtility.GetLastRect();
            UnityObject uObj = EditorGUIExtension.DragDropAreaSingle(rect, DragAndDropVisualMode.Copy);
            if (uObj != null && AssetDatabase.IsMainAsset(uObj))
            {
                string p = AssetDatabase.GetAssetPath(uObj);
                if (AssetDatabase.IsValidFolder(p))
                    _folder.stringValue = p;
            }
            if (GUILayout.Button(EditorGUIUtility.FindTexture("FolderEmpty Icon"), EditorStylesExtension.OnlyIconButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
            {
                string p = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(p))
                    _folder.stringValue = p.Substring(p.IndexOf("Assets"));
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
