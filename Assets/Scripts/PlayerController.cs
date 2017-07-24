using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float movementSpeed = 5f;
    public float sensitivityX = 10f;
    public float sensitivityY = 10f;
    public float jumpHeight = 2f;
    float distToGround;
    Rigidbody rb;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public GameObject cameraGO;
    public float minimumY = -90f;
    public float maximumY = 90f;
    float rotationY = 0F;
    public float reach = 4f;
    public BlockManager blockManager;
    public LayerMask mask;
    bool mouse1down = false;
    bool mouse2down = false;
    
    void Start () {
        rb = GetComponent<Rigidbody>();
        distToGround = GetComponent<Collider>().bounds.extents.y;
    }

    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            mouse1down = true;
        }

        if (Input.GetMouseButtonDown(1)) {
            mouse2down = true;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (Cursor.lockState == CursorLockMode.None) {
                Cursor.lockState = CursorLockMode.Locked;
            } else {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
    
    void FixedUpdate () {
        float y = Input.GetAxis("Vertical") * movementSpeed;
        float x = Input.GetAxis("Horizontal") * movementSpeed;

        rb.velocity = transform.TransformDirection(new Vector3(x, rb.velocity.y, y));

        float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

        transform.localEulerAngles = new Vector3(0, rotationX, 0);
        cameraGO.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
        Debug.DrawRay(cameraGO.transform.position, cameraGO.transform.rotation.eulerAngles);
        if (mouse1down) {
            RaycastHit hit;
            Ray ray = cameraGO.GetComponent<Camera>().ScreenPointToRay(new Vector3(cameraGO.GetComponent<Camera>().pixelWidth/2, cameraGO.GetComponent<Camera>().pixelHeight / 2, 0f));  //new Ray(cameraGO.transform.position, cameraGO.transform.rotation.eulerAngles);
            if(Physics.Raycast(ray, out hit, reach, mask)) {
                blockManager.BlockClick(false, hit);
            }
            mouse1down = false;
        }
        if (mouse2down) {
            RaycastHit hit;
            Ray ray = cameraGO.GetComponent<Camera>().ScreenPointToRay(new Vector3(cameraGO.GetComponent<Camera>().pixelWidth / 2, cameraGO.GetComponent<Camera>().pixelHeight / 2, 0f));  //new Ray(cameraGO.transform.position, cameraGO.transform.rotation.eulerAngles);
            if (Physics.Raycast(ray, out hit, reach, mask)) {
                blockManager.BlockClick(true, hit);
            }
            mouse2down = false;
        }

        if (Input.GetButton("Jump")) {
            if (isGrounded()) {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + CalculateJumpVerticalSpeed(), rb.velocity.z);
            }
        }
    }

    bool isGrounded() {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    private float CalculateJumpVerticalSpeed() {
        return Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics.gravity.y));
    }
    
}
