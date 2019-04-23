using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField]
    private ControllerTransformTracker _controllerTransformTracker;

    [SerializeField]
    private GameObject _menuParent;

    private Transform _cameraTransform;

    [SerializeField]
    private float _minShowingAngle = 130f;

    private float _currentAngle = 0f;

    private bool _menuIsShowing = false;

    private bool _isTracking = false;

    //private bool _isTrackingLeft = false;
    //private bool _isTrackingRight = false;

    private void Start()
    {
        ShowHideMenu(false);
        _cameraTransform = Camera.main.transform;

        // TODO: Add tracking events callbacks
        // TODO: Determine which controller tracker this script is linked to and only listen to those tracking events
        // TODO: Implement that only one menu can show at the same time
        // TODO: Also implement button presses (including affordances!)
        // TODO: Re-check David's and Dan's video

        //_controllerTransformTracker.LeftTrackingStarted += OnLeftTrackingStarted;
        //_controllerTransformTracker.RightTrackingStarted += OnRightTrackingStarted;
    }

    //private void OnLeftTrackingStarted()
    //{
    //    _isTrackingLeft = true;
    //}

    //private void OnRightTrackingStarted()
    //{
    //    _isTrackingRight = true;
    //}

    //private void OnLeftTrackingLost()
    //{
    //    _isTrackingLeft = false;

    //    if (_menuIsShowing)
    //    {
    //        ShowHideMenu(false);
    //    }
    //}

    //private void OnRightTrackingLost()
    //{
    //    _isTrackingRight = false;

    //    if (_menuIsShowing)
    //    {
    //        ShowHideMenu(false);
    //    }
    //}

    private void Update()
    {
        //if (!_isTracking) { return; }

        _currentAngle = CalculateAngle();

        if (_currentAngle > _minShowingAngle && !_menuIsShowing)
        {
            ShowHideMenu(true);
        }
        else if (_currentAngle < _minShowingAngle && _menuIsShowing)
        {
            ShowHideMenu(false);
        }
    }

    private void ShowHideMenu(bool show)
    {
        _menuParent.SetActive(show);
        _menuIsShowing = show;
    }

    private float CalculateAngle()
    {
        float angleCos = Vector3.Dot(_menuParent.transform.forward, _cameraTransform.forward);

        float angle = Mathf.Acos(angleCos);
        angle = angle * Mathf.Rad2Deg;

        Debug.Log("Angle cosine = " + angleCos.ToString());
        Debug.Log("Current angle = " + angle.ToString());

        return angle;
    }
}