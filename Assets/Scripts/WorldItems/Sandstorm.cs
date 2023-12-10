using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandstorm : MonoBehaviour
{
    [SerializeField] Transform playerPos;
    public Vector3 bounds;
    public float speed = 2f;

    private Vector3 destination;
    private float angle = 0f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = RandLoc();

        angle = Random.Range(-60, 60);
        destination = RandLoc();
    }

    // Update is called once per frame
    void Update()
    {
        if((transform.position - destination).magnitude < 100f)
        {
            destination = RandLoc();
            angle = Random.Range(-60, 60);
        }
        Vector3 direction = (destination - transform.position).normalized;
        direction = Quaternion.Euler(0, angle, 0) * direction;
        transform.position += direction * speed * Time.deltaTime;
    }

    private Vector3 RandLoc()
    {
        return new Vector3(Random.Range(playerPos.position.x - (bounds.x / 2), playerPos.position.x + (bounds.x / 2)), transform.position.y, Random.Range(playerPos.position.x - (bounds.x / 2), playerPos.position.x + (bounds.x / 2)));
    }
}
