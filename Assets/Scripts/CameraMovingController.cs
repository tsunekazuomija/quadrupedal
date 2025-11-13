using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovingController : MonoBehaviour
{
    // Update is called once per frame
    void FixedUpdate()
    {
        // wasd位置移動、矢印キー回転
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * 0.1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * 0.1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * 0.1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * 0.1f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position -= transform.up * 0.1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.position += transform.up * 0.1f;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Rotate(new Vector3(-1, 0, 0));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Rotate(new Vector3(1, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(new Vector3(0, -1, 0));
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(new Vector3(0, 1, 0));
        }

        // z回転は常に0にする
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }
}
