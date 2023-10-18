using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class PlacementWithManySinglePrefabSelectionController : MonoBehaviour
{
    [SerializeField]
    private GameObject placedPrefab;

    [SerializeField]
    private GameObject welcomePanel;

    [SerializeField]
    private Button dismissButton;

    [SerializeField]
    private Button arGreenButton;

    [SerializeField]
    private Button arRedButton;

    [SerializeField]
    private Button screenshotButton;

    [SerializeField]
    private Button arBlueButton;

    [SerializeField]
    private Toggle deleteButton;

    [SerializeField]
    private Text selectionText;

    [SerializeField]
    private Camera arCamera;

    private Vector2 touchPosition = default;
    private ARRaycastManager arRaycastManager;
    private bool onTouchHold = false;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private PlacementObject lastSelectedObject;

    private const string PREFAB_CHAIR = "ChairRotation";
    private const string PREFAB_COUCH = "CouchRotation";
    private const string PREFAB_CLOSET = "ClosetRotation";
    private const string PREFAB_FAUCET = "FaucetRotation";

    private GameObject PlacedPrefab 
    {
        get 
        {
            return placedPrefab;
        }
        set 
        {
            placedPrefab = value;
        }
    }


    void Awake() 
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arGreenButton.onClick.AddListener(() => ChangePrefabTo(PREFAB_CHAIR));
        arBlueButton.onClick.AddListener(() => ChangePrefabTo(PREFAB_COUCH));
        arRedButton.onClick.AddListener(() => ChangePrefabTo(PREFAB_CLOSET));
        screenshotButton.onClick.AddListener(() => TakeScreenshot());
        //dismissButton.onClick.AddListener(Dismiss);
    }

    void ChangePrefabTo(string prefabName)
    {
        placedPrefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");

        if(placedPrefab == null)
        {
            Debug.LogError($"Prefab with name {prefabName} could not be loaded, make sure you check the naming of your prefabs...");
        }
        
        switch(prefabName)
        {
            case PREFAB_CHAIR:
                selectionText.text = "Selected: Chair";
            break;
            case PREFAB_COUCH:
                selectionText.text = "Selected: Couch";
            break;
            case PREFAB_CLOSET:
                selectionText.text = "Selected: Closet";
            break;
        }
    }

    void TakeScreenshot() {
        ScreenCapture.CaptureScreenshot("MovelRA.png");
    } 

    private void Dismiss() => welcomePanel.SetActive(false);

    void Update()
    {
        Debug.Log(deleteButton.isOn);
        // do not capture events unless the welcome panel is hidden
        if(welcomePanel.activeSelf)
            return;

        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            touchPosition = touch.position;

            if(touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if(Physics.Raycast(ray, out hitObject))
                {
                    lastSelectedObject = hitObject.transform.GetComponent<PlacementObject>();
                    if(lastSelectedObject != null)
                    {
                        if (deleteButton.isOn)
                        {
                            Destroy(hitObject.transform.gameObject);
                        } 
                        else
                        {
                            PlacementObject[] allOtherObjects = FindObjectsOfType<PlacementObject>();
                            foreach(PlacementObject placementObject in allOtherObjects)
                            {
                                if(placementObject != lastSelectedObject){
                                    placementObject.Selected = false;
                                }
                                else
                                    placementObject.Selected = true;
                            }
                        }
                    }
                }
                if(arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;

                    if(lastSelectedObject == null && !deleteButton.isOn)
                    {
                        lastSelectedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation).GetComponent<PlacementObject>();
                    }
                }
            }  

            if(touch.phase == TouchPhase.Moved)
            {
                if(arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;

                    if(lastSelectedObject != null && lastSelectedObject.Selected)
                    {
                        lastSelectedObject.transform.parent.position = hitPose.position;
                        lastSelectedObject.transform.parent.rotation = hitPose.rotation;
                    }
                }
            }
        }
    }
}
