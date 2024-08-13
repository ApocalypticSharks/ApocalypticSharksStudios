using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Language : MonoBehaviour
{
    public string language;
    public static Language instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            language = Yandex.instance.SelectedLanguage();
        }
        else 
        {
            Destroy(gameObject);
        }
    }
}
