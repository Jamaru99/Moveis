using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class PlacementWithManySinglePrefabSelectionController : MonoBehaviour
{
    private GameObject placedPrefab;

    [SerializeField]
    private Button chairButton;

    [SerializeField]
    private Button closetButton;

    [SerializeField]
    private Button couchButton;

    [SerializeField]
    private Button faucetButton;

    [SerializeField]
    private Button screenshotButton;

    [SerializeField]
    private Toggle deleteButton;

    [SerializeField]
    private GameObject selectedSquare;

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
        chairButton.onClick.AddListener(() => ChangePrefabTo(PREFAB_CHAIR, chairButton.transform));
        couchButton.onClick.AddListener(() => ChangePrefabTo(PREFAB_COUCH, couchButton.transform));
        closetButton.onClick.AddListener(() => ChangePrefabTo(PREFAB_CLOSET, closetButton.transform));
        faucetButton.onClick.AddListener(() => ChangePrefabTo(PREFAB_FAUCET, faucetButton.transform));
        screenshotButton.onClick.AddListener(() => TakeScreenshot());
        
        ChangePrefabTo(PREFAB_COUCH, couchButton.transform);
    }

    void ChangePrefabTo(string prefabName, Transform buttonPosition)
    {
        placedPrefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");

        if(placedPrefab == null)
        {
            Debug.LogError($"Prefab with name {prefabName} could not be loaded, make sure you check the naming of your prefabs...");
        }

        selectedSquare.transform.SetParent(buttonPosition);
        selectedSquare.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }

    void TakeScreenshot() {
        ScreenCapture.CaptureScreenshot("MovelRA.png");
    }

    void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            touchPosition = touch.position;

            if(touchPosition.y < Screen.height / 4)
                return;

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
