using UnityEngine;
using TMPro;
using System.Collections;

public class TunkuPhotoScript : MonoBehaviour
{
    private const float DEFAULT_VIEW_DISTANCE = 2.0f;

    [SerializeField]
    private TextMeshProUGUI dialogue;
    public string[] TextToBeShown;

    [Header("Camera Settings")]
    [Tooltip("How far from the picture the camera should be positioned during viewing.")]
    [SerializeField] private float viewDistance = DEFAULT_VIEW_DISTANCE;

    private int TotalLength;

    private bool once = false;
    private bool InRange = false;
    private bool Pressed = false;
    private bool waitingForNextLine = false;
    private PhotoCameraController cameraController;

    private void Awake()
    {
        TotalLength = TextToBeShown.Length;
        cameraController = FindAnyObjectByType<PhotoCameraController>();
    }

    private void Update()
    {
        if (!InRange)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !Pressed)
        {
            PlayerScript.IsFreezed = true;
            Pressed = true;
        }

        if (!Pressed)
            return;

        if (Input.GetKeyDown(KeyCode.Space) && waitingForNextLine)
        {
            waitingForNextLine = false;
        }

        if (!once)
        {
            once = true;
            StartCoroutine(HandlePhotoInteraction());
        }
    }

    private IEnumerator HandlePhotoInteraction()
    {
        dialogue.text = "";

        // Pan camera to the photo
        bool cameraDone = false;
        if (cameraController != null)
        {
            cameraController.FocusOnPhoto(transform, viewDistance, () => cameraDone = true);
            yield return new WaitUntil(() => cameraDone);
        }

        // Show dialogue lines one by one
        for (int i = 0; i < TotalLength; i++)
        {
            dialogue.text = TextToBeShown[i];
            waitingForNextLine = true;
            yield return new WaitUntil(() => !waitingForNextLine);
            // Wait one frame so the same key press isn't consumed twice
            yield return null;
        }

        dialogue.text = "";

        // Pan camera back to the player view
        cameraDone = false;
        if (cameraController != null)
        {
            cameraController.ReturnToPlayer(() => cameraDone = true);
            yield return new WaitUntil(() => cameraDone);
        }

        dialogue.text = "Press SpaceBar to show text.";
        Pressed = false;
        once = false;
        PlayerScript.IsFreezed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            dialogue.text = "Press SpaceBar to show text.";
            InRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            dialogue.text = "";
            InRange = false;
            once = false;
            Pressed = false;
            waitingForNextLine = false;
            StopAllCoroutines();

            // If camera was mid-transition, snap it back immediately
            if (cameraController != null)
            {
                cameraController.ForceReturn();
            }

            PlayerScript.IsFreezed = false;
        }
    }
}
