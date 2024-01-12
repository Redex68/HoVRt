using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableIfNotExist : MonoBehaviour
{
    [SerializeField] List<GameObject> toEnable;
    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject obj in toEnable)
        {
            if(GameObject.Find(obj.name) == null) obj.SetActive(true);
        }

        Destroy(this);
    }
}
