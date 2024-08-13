using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InternationalText : MonoBehaviour
{
    [SerializeField] private string _ru;
    [SerializeField] private string _en;
    [SerializeField] private string _tr;
    [SerializeField] private Text _text;
    private void Start()
    {
        _text = GetComponent<Text>();
        switch (Language.instance.language)
        {
            case "ru":
                _text.text = _ru;
                break;
            case "en":
                _text.text = _en;
                break;
            case "tr":
                _text.text = _tr;
                break;
            default:
                _text.text = _en;
                break;
        }
    }
}
