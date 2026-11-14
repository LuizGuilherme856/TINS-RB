using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechBooh
{



    /// <summary>
    /// Helpers that find components and GameObjects in the scene.
    /// Throw a descriptive exception when the requested object is missing.
    /// </summary>
    public static class Try
    {
        /// <summary>
        /// Get component T from the GameObject.
        /// Throw MissingComponentException if missing.
        /// </summary>
        public static T GetComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"O objeto \"{obj.name}\" n�o possui o componente do tipo {typeof(T).Name}.");
                // throw new MissingComponentException(
                // $"Object \"{obj.name}\" has no component of type {typeof(T).Name}.");
            }
            return component;
        }

        /// <summary>
        /// Get all components of type T from the GameObject.
        /// Throw MissingComponentException if none are found.
        /// </summary>
        public static T[] GetComponents_Plural<T>(GameObject obj) where T : Component
        {
            T[] components = obj.GetComponents<T>();
            if (components == null || components.Length == 0)
            {
                Debug.LogError($"O objeto \"{obj.name}\" n�o possui nenhum componente do tipo {typeof(T).Name}.");
                // throw new MissingComponentException(
                // $"Object \"{obj.name}\" has no components of type {typeof(T).Name}.");
            }
            return components;
        }

        /// <summary>
        /// Find an object of type T in the scene.
        /// Throw an exception if not found.
        /// </summary>
        public static T FindObjectOfType<T>() where T : Object
        {
            T obj = Object.FindFirstObjectByType<T>();
            if (obj == null)
            {
                Debug.LogError($"Nenhum objeto do tipo {typeof(T).Name} foi encontrado na cena.");
                // throw new System.Exception($"No object of type {typeof(T).Name} was found in the scene.");
            }
            return obj;
        }

        /// <summary>
        /// Find all objects of type T in the scene.
        /// Throw an exception if none are found.
        /// </summary>
        public static T[] FindObjectsOfType_Plural<T>() where T : Object
        {
            T[] objs = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            if (objs == null || objs.Length == 0)
            {
                Debug.LogError($"Nenhum objeto do tipo {typeof(T).Name} foi encontrado na cena.");
                // throw new System.Exception($"No object of type {typeof(T).Name} was found in the scene.");
            }
            return objs;
        }

        /// <summary>
        /// Find all GameObjects with the specified tag.
        /// Throw an exception if none are found.
        /// </summary>
        public static GameObject[] FindGameObjectsWithTag_Plural(string tag)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            if (objs == null || objs.Length == 0)
            {
                Debug.LogError($"Nenhum GameObject com a tag \"{tag}\" foi encontrado na cena.");
                // throw new System.Exception($"No GameObject with tag \"{tag}\" was found in the scene.");
            }
            return objs;
        }

        /// <summary>
        /// Find a GameObject with the specified tag.
        /// Throw an exception if not found.
        /// </summary>
        public static GameObject FindGameObjectWithTag(string tag)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(tag);
            if (obj == null)
            {
                Debug.LogError($"Nenhum GameObject com a tag \"{tag}\" foi encontrado na cena.");
                // throw new System.Exception($"No GameObject with tag \"{tag}\" was found in the scene.");
            }
            return obj;
        }
    }
}

