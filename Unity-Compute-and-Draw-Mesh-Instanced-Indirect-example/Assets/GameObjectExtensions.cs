using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public static class GameObjectExtensions
{
    public static void SetParentAndZero(this GameObject gO, GameObject parent)
    {
        gO.transform.parent = parent.transform;

        gO.transform.localPosition = Vector3.zero;
        gO.transform.localRotation = Quaternion.identity;
        gO.transform.localScale = Vector3.one;
    }

    public static void SetParentAndZero(this GameObject gO, Transform parent)
    {
        gO.transform.parent = parent;

        gO.transform.localPosition = Vector3.zero;
        gO.transform.localRotation = Quaternion.identity;
        gO.transform.localScale = Vector3.one;
    }

    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }

    public static void DestroyAllChildren(this GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            UnityEngine.Object.DestroyImmediate(obj.transform.GetChild(i).gameObject);
        }
    }

    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public| BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }

}
