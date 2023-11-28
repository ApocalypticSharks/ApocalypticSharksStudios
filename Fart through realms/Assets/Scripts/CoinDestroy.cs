using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinDestroy : MonoBehaviour
{
    private void DestroyCoin()
    { 
        Destroy(transform.parent.gameObject);
    }
}
