using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestorySingleton<T> : MonoBehaviour where T : DontDestorySingleton<T>
{
    private static T instance;

    public static T Instance { get { return instance; } }

    public static bool IsInitialized()
    {
        return instance != null;
    }

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = (T)this;
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

