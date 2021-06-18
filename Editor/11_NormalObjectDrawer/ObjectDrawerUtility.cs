﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace CZToolKit.Core.Editors
{
    public static class ObjectDrawerUtility
    {
        private static void BuildObjectDrawers()
        {
            if (ObjectDrawerUtility.mapBuilt)
            {
                return;
            }
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly != null)
                {
                    try
                    {
                        foreach (Type type in assembly.GetExportedTypes())
                        {
                            CustomFieldDrawerAttribute[] array;
                            if (typeof(FieldDrawer).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract && (array = (type.GetCustomAttributes(typeof(CustomFieldDrawerAttribute), false) as CustomFieldDrawerAttribute[])).Length > 0)
                            {
                                ObjectDrawerUtility.objectDrawerTypeMap.Add(array[0].Type, type);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            ObjectDrawerUtility.mapBuilt = true;
        }

        private static bool ObjectDrawerForType(Type _fieldType, ref FieldDrawer _fieldDrawer, ref Type _fieldDrawerType, int _hash)
        {
            ObjectDrawerUtility.BuildObjectDrawers();
            if (!ObjectDrawerUtility.objectDrawerTypeMap.ContainsKey(_fieldType))
            {
                return false;
            }
            _fieldDrawerType = ObjectDrawerUtility.objectDrawerTypeMap[_fieldType];
            if (ObjectDrawerUtility.objectDrawerMap.ContainsKey(_hash))
            {
                _fieldDrawer = ObjectDrawerUtility.objectDrawerMap[_hash];
            }
            return true;
        }

        public static FieldDrawer GetObjectDrawer(FieldInfo _fieldInfo)
        {
            FieldDrawer objectDrawer = null;
            Type type = null;
            if (!ObjectDrawerUtility.ObjectDrawerForType(_fieldInfo.FieldType, ref objectDrawer, ref type, _fieldInfo.GetHashCode()))
                return null;
            if (objectDrawer == null)
            {
                objectDrawer = (Activator.CreateInstance(type) as FieldDrawer);
                ObjectDrawerUtility.objectDrawerMap.Add(_fieldInfo.GetHashCode(), objectDrawer);
            }
            objectDrawer.FieldInfo = _fieldInfo;
            return objectDrawer;
        }

        public static FieldDrawer GetObjectDrawer(FieldAttribute attribute)
        {
            FieldDrawer objectDrawer = null;
            Type type = null;
            if (!ObjectDrawerUtility.ObjectDrawerForType(attribute.GetType(), ref objectDrawer, ref type, attribute.GetHashCode()))
                return null;
            if (objectDrawer != null)
                return objectDrawer;
            objectDrawer = (Activator.CreateInstance(type) as FieldDrawer);
            objectDrawer.Attribute = attribute;
            ObjectDrawerUtility.objectDrawerMap.Add(attribute.GetHashCode(), objectDrawer);
            return objectDrawer;
        }

        private static Dictionary<Type, Type> objectDrawerTypeMap = new Dictionary<Type, Type>();

        private static Dictionary<int, FieldDrawer> objectDrawerMap = new Dictionary<int, FieldDrawer>();

        private static bool mapBuilt = false;
    }
}
