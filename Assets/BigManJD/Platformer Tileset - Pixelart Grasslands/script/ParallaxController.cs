using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    Transform cam; // Main Camera
    Vector3 camStartPos;
    float distance; // Jarak antara posisi kamera awal dan posisi kamera saat ini

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;
    Vector2[] currentOffset; // Tambahan: offset saat ini (untuk smoothing)

    float farthestBack;

    [Range(0.01f, 0.05f)]
    public float parallaxSpeed;

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];
        currentOffset = new Vector2[backCount]; // Inisialisasi array offset

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
            currentOffset[i] = Vector2.zero; // Mulai dari offset 0
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++)
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++)
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;

        // Tetap ikuti kamera secara horizontal
        transform.position = new Vector3(cam.position.x, cam.position.y, 0);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            Vector2 targetOffset = new Vector2(distance * speed, 0f); // Offset hanya di X

            // Loop offset agar seamless
            targetOffset.x = targetOffset.x % 1f;

            // Smooth offset dengan Lerp
            currentOffset[i] = Vector2.Lerp(currentOffset[i], targetOffset, Time.deltaTime * 5f); // 5f = kecepatan smoothing

            mat[i].SetTextureOffset("_MainTex", currentOffset[i]);
        }
    }
}
