using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField]
    private ControllerTransformTracker _controllerTransformTracker;

    [SerializeField]
    private GameObject _menuParent;

    [SerializeField]
    private float _minShowingAngle = 130f;

    private AttachToControllerSolver _attachToControllerSolver;
    private HandMenuManager _handMenuManager;
    private float _currentAngle = 0f;
    private Transform _cameraTransform;

    public bool MenuIsShowingOnThisHand { get; private set; } = false;

    private void Start()
    {
        ShowHideMenu(false);

        _handMenuManager = FindObjectOfType<HandMenuManager>();

        _attachToControllerSolver = GetComponent<AttachToControllerSolver>();
        _attachToControllerSolver.TrackingLost += OnTrackingLost;

        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (_attachToControllerSolver.IsTracking)
        {
            _currentAngle = CalculateAngle();

            if (_currentAngle > _minShowingAngle && !MenuIsShowingOnThisHand)
            {
                // Check if the menu is already showing on the other hand
                if (!_handMenuManager.MenuIsAlreadyShowing)
                {
                    ShowHideMenu(true);
                }
            }
            else if (_currentAngle < _minShowingAngle && MenuIsShowingOnThisHand)
            {
                ShowHideMenu(false);
            }
        }
    }

    private void OnTrackingLost()
    {
        ShowHideMenu(false);
    }

    private void ShowHideMenu(bool show)
    {
        _menuParent.SetActive(show);
        MenuIsShowingOnThisHand = show;
    }

    private float CalculateAngle()
    {
        float angleCos = Vector3.Dot(_menuParent.transform.forward, _cameraTransform.forward);

        float angle = Mathf.Acos(angleCos);
        angle = angle * Mathf.Rad2Deg;

        return angle;
    }
}