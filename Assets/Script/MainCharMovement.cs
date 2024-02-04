using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainCharMovement : MonoBehaviour
{
    [Header("Character Utils")]
    public Rigidbody rb;
    public float speed, rotationSpeed;
    public Transform playerCamera;
    public CinemachineFreeLook camFreeLook;
    public float rotationSpeedX = 2f;
    public float rotationSpeedY = 0.5f;

    [Header("Interaction Utils")]
    public float interactRadius = 2f;
    public TextMeshProUGUI cube, sphere, totalSampah;
    public int cubeVal = 0, sphereVal = 0, totalVal = 0;
    public GameObject[] TrashBagObj;
    public TextMeshProUGUI notificationPanel;
    public bool isCanInteractTrash = true;

    [Header("Joystick Utils")]
    public bool enableMobileInput = false;
    public FixedJoystick joystick;
    public FixedTouchField touchField;

    [Header("Screen Service")]
    public GameObject loadingPanel;
    public GameObject shopPanel;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;


        notificationPanel.gameObject.SetActive(false);
        loadingPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithNearbyItem();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            InteractWithTrashcan();
        }

        cube.text = cubeVal.ToString();
    }

    void FixedUpdate()
    {
        Vector2 input = Vector2.zero;
        if (enableMobileInput)
        {
            //camFreeLook.enabled = false;

            float rotationX = touchField.TouchDist.x * rotationSpeedX * Time.deltaTime;
            float rotationY = touchField.TouchDist.y * rotationSpeedY * Time.deltaTime;

            camFreeLook.m_XAxis.Value += rotationX;
            camFreeLook.m_YAxis.Value -= rotationY;

            // IMPLEMENTASI JOYSTICK
            float x = joystick.Horizontal;
            float z = joystick.Vertical;

            // Calculate movement direction relative to the camera
            Vector3 cameraForward = playerCamera.forward;
            Vector3 cameraRight = playerCamera.right;

            // Flatten the vectors so the character doesn't move up and down
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            Vector3 desiredMoveDirection = (cameraForward.normalized * z + cameraRight.normalized * x).normalized;

            // Move and rotate the character
            MoveCharacter(desiredMoveDirection);
            RotateCharacter(desiredMoveDirection);
        }
        else
        {
            //camFreeLook.enabled = true;
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeedX;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeedY;

            camFreeLook.m_XAxis.Value += rotationX;
            camFreeLook.m_YAxis.Value -= rotationY;

            // Get input from arrow keys or WASD
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // Calculate movement direction relative to the camera
            Vector3 cameraForward = playerCamera.forward;
            Vector3 cameraRight = playerCamera.right;

            // Flatten the vectors so the character doesn't move up and down
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            Vector3 desiredMoveDirection = (cameraForward.normalized * verticalInput + cameraRight.normalized * horizontalInput).normalized;

            // Move and rotate the character
            MoveCharacter(desiredMoveDirection);
            RotateCharacter(desiredMoveDirection);
        }
    }

    private void MoveCharacter(Vector3 direction)
    {
        // Move the character using Rigidbody
        Vector3 targetPosition = transform.position + direction * speed * Time.deltaTime;
        rb.MovePosition(Vector3.Lerp(transform.position, targetPosition, 0.1f));
    }

    private void RotateCharacter(Vector3 direction)
    {
        // Rotate the character to face the movement direction
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime));
        }
    }

    public void InteractWithNearbyItem()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactRadius);

        GameObject closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in nearbyColliders)
        {
            if (collider.CompareTag("Item"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = collider.gameObject;
                }
            }
        }

        if (closestItem != null && closestDistance <= interactRadius)
        {

            TrashController itemPickup = closestItem.GetComponent<TrashController>();

            if (itemPickup != null)
            {
                if (isCanInteractTrash)
                {
                    itemPickup.Interact(closestItem);
                    Debug.Log("Item interacted!");

                    if (totalVal < 7)
                    {
                        if (closestItem.transform.parent.gameObject.name == "11")
                        {
                            cubeVal++;
                            cube.text = cubeVal.ToString();
                        }
                        else if (closestItem.transform.parent.gameObject.name == "2")
                        {
                            sphereVal++;
                            sphere.text = sphereVal.ToString();
                        }

                        totalVal = cubeVal + sphereVal;
                        totalSampah.text = totalVal.ToString();

                        if (totalVal <= 2)
                            SetTrashBagState(TrashBagObj, true, false, false);
                        else if (totalVal < 5)
                            SetTrashBagState(TrashBagObj, false, true, false);
                        else if (totalVal <= 6)
                            SetTrashBagState(TrashBagObj, false, false, true);
                        else
                            isCanInteractTrash = false;
                    }
                    else
                    {
                        //
                    }
                }
                else
                {
                    DisableInteractAndShowNotification(notificationPanel, "Kantong Sampah Penuh", 3);
                }
            }
        }
    }

    private void SetTrashBagState(GameObject[] TrashBagObj, bool bag1, bool bag2, bool bag3)
    {
        TrashBagObj[0].SetActive(bag1);
        TrashBagObj[1].SetActive(bag2);
        TrashBagObj[2].SetActive(bag3);
    }

    private void DisableInteractAndShowNotification(TextMeshProUGUI notificationPanel, string notificationText, float delayTime)
    {
        Debug.Log("Penuhhhh");
        StartCoroutine(time_delay(notificationPanel, delayTime, notificationText));
    }

    IEnumerator time_delay(TextMeshProUGUI notificationPanel, float delayTime, string notificationText)
    {
        notificationPanel.gameObject.SetActive(true);
        notificationPanel.text = notificationText;
        yield return new WaitForSeconds(delayTime);
        notificationPanel.gameObject.SetActive(false);
    }


    private void InteractWithTrashcan()
    {
        Collider[] nearbyTrashcan = Physics.OverlapSphere(transform.position, interactRadius);

        GameObject closestTrashcan = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in nearbyTrashcan)
        {
            if (collider.CompareTag("Trashcan"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTrashcan = collider.gameObject;
                }
            }
        }

        if (closestTrashcan != null)
        {
            if (closestDistance <= interactRadius)
            {
                Debug.Log("Item terdekat" + closestTrashcan.name);
                totalVal = 0;
                totalSampah.text = totalVal.ToString();
                cubeVal = 0;
                cube.text = cubeVal.ToString();
                sphereVal = 0;
                sphere.text = sphereVal.ToString();
                isCanInteractTrash = true;

                if (TrashBagObj != null)
                {
                    foreach (GameObject obj in TrashBagObj)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    //public void SavePlayer()
    //{
    //    SaveSystem.SavePlayer(this);
    //}

    public void LoadPlayer()
    {
        StartCoroutine(LoadPlayerCoroutine());
    }

    private IEnumerator LoadPlayerCoroutine()
    {
        // Tampilkan layar loading di sini
        loadingPanel.SetActive(true);
        loadingPanel.transform.localPosition = new Vector3(0f, 0f, 0f);

        yield return null; // Delay untuk memberikan kesempatan layar loading untuk tampil

        PlayerData data = SaveSystem.LoadPlayer();

        if (data != null)
        {
            // Set position
            Vector3 position = new Vector3(data.position[0], data.position[1], data.position[2]);
            transform.position = position;

            // Set rotation
            Quaternion rotation = Quaternion.Euler(data.rotation[0], data.rotation[1], data.rotation[2]);
            transform.rotation = rotation;

            Debug.Log("Posisi" + position);
            Debug.Log("Rotasi" + rotation);
        }

        // Sembunyikan layar loading di sini
        loadingPanel.SetActive(false);
    }

    public void ShowShopPanel()
    {
        shopPanel.SetActive(true);
        shopPanel.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

    public void CloseShopPanel()
    {
        shopPanel.SetActive(false);
    }
}