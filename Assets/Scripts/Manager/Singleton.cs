using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);
                obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }
}
