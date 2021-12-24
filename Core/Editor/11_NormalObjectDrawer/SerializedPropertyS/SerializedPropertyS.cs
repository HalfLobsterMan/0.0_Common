#region ע ��
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: ��ֻ��Ϻ��
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.Core.Editors
{
    public class SerializedPropertyS
    {
        // ֱ�Ӷ���
        private object value;

        private Dictionary<string, SerializedPropertyS> childrens;

        // �����ĺ�FieldInfo
        public readonly SerializedPropertyS parent;
        public readonly FieldInfo fieldInfo;

        /// <summary> �Ƿ�չ�� </summary>
        public bool expanded;
        public readonly Type propertyType;
        public readonly GUIContent niceName;
        public readonly bool isArray;
        public readonly bool hasChildren;

        public object Value
        {
            get { return value; }
            set
            {
                if (value != null)
                {
                    var tempType = value.GetType();
                    if (!propertyType.IsAssignableFrom(tempType))
                        throw new Exception($"{nameof(value)}({tempType.Name})����{propertyType.Name}����");
                }
                this.value = value;
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(parent.Value, this.value);
                }
            }
        }
        public bool IsRoot
        {
            get { return parent == null; }
        }
        public bool HasChildren
        {
            get { return hasChildren; }
        }

        public SerializedPropertyS(object value)
        {
            if (value == null)
                throw new NullReferenceException($"{nameof(value)}����Ϊ�գ�");

            this.value = value;
            this.niceName = GUIHelper.TextContent(value.GetType().Name);
            this.propertyType = value.GetType();
            this.isArray = typeof(IList).IsAssignableFrom(propertyType);

            if (EditorGUIExtension.IsBasicType(propertyType))
                hasChildren = false;
            else if (propertyType.IsClass || (propertyType.IsValueType && !propertyType.IsPrimitive))
            {
                if (!typeof(Delegate).IsAssignableFrom(propertyType) && typeof(object).IsAssignableFrom(propertyType))
                {
                    hasChildren = true;
                }
            }
            if (!HasChildren)
            {
                childrens = new Dictionary<string, SerializedPropertyS>();
                if (!typeof(UnityObject).IsAssignableFrom(propertyType))
                    fieldInfo.SetValue(parent, EditorGUIExtension.CreateInstance(propertyType));
            }
        }

        private SerializedPropertyS(FieldInfo fieldInfo, SerializedPropertyS parent)
        {
            this.parent = parent;
            this.fieldInfo = fieldInfo;
            this.value = fieldInfo.GetValue(parent.Value);
            this.propertyType = fieldInfo.FieldType;
            this.niceName = GUIHelper.TextContent(ObjectNames.NicifyVariableName(fieldInfo.Name));
            this.isArray = typeof(IList).IsAssignableFrom(propertyType);

            if (EditorGUIExtension.IsBasicType(propertyType))
                hasChildren = false;
            else if (propertyType.IsClass || (propertyType.IsValueType && !propertyType.IsPrimitive))
            {
                if (!typeof(Delegate).IsAssignableFrom(propertyType) && typeof(object).IsAssignableFrom(propertyType))
                {
                    hasChildren = true;
                }
            }
            if (!HasChildren)
            {
                childrens = new Dictionary<string, SerializedPropertyS>();
                if (!typeof(UnityObject).IsAssignableFrom(propertyType))
                    fieldInfo.SetValue(parent.Value, EditorGUIExtension.CreateInstance(propertyType));
            }
        }

        private void InternalCreate()
        {
            if (value == null)
            {
                value = EditorGUIExtension.CreateInstance(propertyType);
                fieldInfo.SetValue(parent.Value, value);
            }

            childrens = new Dictionary<string, SerializedPropertyS>();

            if (isArray)
            {
                var list = (IList)value;
                for (int i = 0; i < list.Count; i++)
                {
                    childrens[$"Element {i}"] = new SerializedPropertyS(list[i]);
                }
            }
            else
            {
                foreach (var fieldInfo in Util_Reflection.GetFieldInfos(propertyType))
                {
                    if (fieldInfo.Name.StartsWith("<"))
                        continue;
                    // public ���η�
                    if (!fieldInfo.IsPublic)
                    {
#if UNITY_EDITOR
                        // ���������SerializeField����
                        if (!Util_Attribute.TryGetFieldAttribute<SerializeField>(fieldInfo, out var serializeField))
                        {
                            continue;
                        }
#endif
                        // ������NonSerialized����
                        if (Util_Attribute.TryGetFieldAttribute<NonSerializedAttribute>(fieldInfo, out var nonSerialized))
                        {
                            continue;
                        }
                        // ������HideInInspector����
                        if (Util_Attribute.TryGetFieldAttribute<HideInInspector>(fieldInfo, out var hideInInspector))
                        {
                            continue;
                        }
                    }
                    childrens[fieldInfo.Name] = new SerializedPropertyS(fieldInfo, this);
                }
            }
        }

        private void Check()
        {
            if (childrens == null)
                InternalCreate();
            if (isArray)
            {
                var list = (IList)value;
                for (int i = 0; i < list.Count; i++)
                {
                    string elementName = $"Element {i}";
                    if (!childrens.TryGetValue(elementName, out var element) || element.Value != list[i])
                    {
                        childrens[elementName] = new SerializedPropertyS(list[i]);
                    }
                }
            }
        }

        public IEnumerable<SerializedPropertyS> GetIterator()
        {
            Check();
            foreach (var pair in childrens)
            {
                yield return pair.Value;
            }
        }
    }
}
