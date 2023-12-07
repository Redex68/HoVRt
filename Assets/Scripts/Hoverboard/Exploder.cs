using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Exploder : MonoBehaviour
{
    [SerializeField] GameObject explosion;
    [SerializeField] LayerMask thingsThatExplodeMe;
    [SerializeField] LayerMask ground;
    [SerializeField] GameObject cam;
    [SerializeField] GameEvent gameOver;

    void OnCollisionEnter(Collision collision)
    {
        if((thingsThatExplodeMe.value & (1 << collision.gameObject.layer)) != 0)
            Explode();
        else if(((ground.value & (1 << collision.gameObject.layer)) != 0) && Vector3.Dot(transform.up, Vector3.up) < 0)
            Explode();
    }

    private void Explode()
    {
        explosion.transform.SetParent(null, true);
        cam.transform.SetParent(null, true);
        explosion.SetActive(true);

        gameOver.SimpleRaise();
        
        Destroy(explosion, 5.0f);
        gameObject.SetActive(false);
    }
}
