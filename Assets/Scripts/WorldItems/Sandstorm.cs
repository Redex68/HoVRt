using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandstorm : MonoBehaviour
{
    public Vector3 center;
    public Vector3 bounds;
    public float speed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        float randX = Random.Range(center.x - (bounds.x / 2), center.x + (bounds.x / 2));
        float randZ = Random.Range(center.z - (bounds.z / 2), center.z + (bounds.z / 2));
        transform.position = new Vector3(randX, transform.position.y, randZ);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = center - transform.position;
        direction = Quaternion.Euler(0, 0, 60) * direction;
        direction.y = 0;
        float distanceThisFrame = speed * Time.deltaTime;

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);

        if (transform.position.x > center.x + (bounds.x / 2))
            transform.position = new Vector3(center.x + (bounds.x / 2), transform.position.y, transform.position.z);
        else if (transform.position.x < center.x - (bounds.x / 2))
            transform.position = new Vector3(center.x - (bounds.x / 2), transform.position.y, transform.position.z);

        if (transform.position.z > center.z + (bounds.z / 2))
            transform.position = new Vector3(transform.position.x, transform.position.y, center.z + (bounds.z / 2));
        else if (transform.position.z < center.z - (bounds.z / 2))
            transform.position = new Vector3(transform.position.x, transform.position.y, center.z - (bounds.z / 2));
    }
}
