using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIClose : MonoBehaviour
{    
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            transform.parent.gameObject.SetActive(false);
        });
    }
}
