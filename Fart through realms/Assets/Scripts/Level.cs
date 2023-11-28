using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private Object levelLayout;
    private GameObject levelView;
    public Level(int realm)
    {
        levelLayout = Resources.Load($"{realm}/levelLayout");
        levelView = Instantiate(levelLayout) as GameObject;
    }
}
