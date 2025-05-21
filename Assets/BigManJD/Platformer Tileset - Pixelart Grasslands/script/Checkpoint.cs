using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    private bool isActivated = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActivated && other.CompareTag("Player"))
        {
            Debug.Log("Checkpoint activated!");
            anim.SetTrigger("Activate");
            isActivated = true;
            other.GetComponent<PlayerMovement>().SetCheckpoint(transform.position);
        }
    }
}

