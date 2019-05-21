using GalaxyExplorer;
using System.Collections;
using UnityEngine;

public class DesktopButtonsManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _menuParent;

    [SerializeField]
    private GameObject _buttonParent;

    [SerializeField]
    private GameObject _backButton;

    [SerializeField]
    private GameObject _resetButton;

    private POIPlanetFocusManager POIPlanetFocusManager
    {
        get
        {
            if (_pOIPlanetFocusManager == null)
            {
                _pOIPlanetFocusManager = FindObjectOfType<POIPlanetFocusManager>();
            }

            return _pOIPlanetFocusManager;
        }
    }

    private POIPlanetFocusManager _pOIPlanetFocusManager;
    private AboutSlate _aboutSlate;
    private Vector3 _defaultBackButtonLocalPosition;
    private float _fullMenuVisibleBackButtonX;
    private Transform _cameraTransform;

    public bool IsVisible { get; private set; } = false;

    private void Start()
    {
        SetMenuVisibility(false);

        _aboutSlate = FindObjectOfType<AboutSlate>();

        // Store the x value of the local position for the back button when all menu buttons are visible
        _fullMenuVisibleBackButtonX = _backButton.transform.localPosition.x;

        // Since reset is not visible during most of the app states, regard its local position as the default back button local position
        _defaultBackButtonLocalPosition = _resetButton.transform.localPosition;

        // Since the app starts with reset button not visible, move the back button to its spot instead
        _backButton.transform.localPosition = _defaultBackButtonLocalPosition;

        if (GalaxyExplorerManager.Instance.ViewLoaderScript)
        {
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnSceneIsLoaded += OnSceneIsLoaded;
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnLoadNewScene += OnLoadNewScene;
        }

        GalaxyExplorerManager.Instance.ToolsManager.BackButtonNeedsShowing += OnBackButtonNeedsToShow;
        _backButton.SetActive(false);
        _resetButton.SetActive(false);

        _cameraTransform = Camera.main.transform;
    }

    private void OnDestroy()
    {
        if (GalaxyExplorerManager.Instance != null && GalaxyExplorerManager.Instance.ViewLoaderScript)
        {
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnSceneIsLoaded -= OnSceneIsLoaded;
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnLoadNewScene -= OnLoadNewScene;
        }
    }

    private void OnSceneIsLoaded()
    {
        //if (GalaxyExplorerManager.Platform != GalaxyExplorerManager.PlatformId.Desktop) { return; }

        if (GalaxyExplorerManager.Instance.TransitionManager.IsInIntroFlow) { return; }

        StartCoroutine(OnSceneIsLoadedCoroutine());
    }

    private IEnumerator OnSceneIsLoadedCoroutine()
    {
        Debug.Log("In OnSceneIsLoadedCoroutine before waiting 1 sec. (POIPlanetFocusManager != null) = " + (POIPlanetFocusManager != null) + " / !_resetButton.activeInHierarchy = " + (!_resetButton.activeInHierarchy));

        // waiting necessary for events in flow manager to be called and
        // stage of intro flow to be correct when executing following code
        yield return new WaitForSeconds(1);

        Debug.Log("In OnSceneIsLoadedCoroutine after waiting 1 sec. (POIPlanetFocusManager != null) = " + (POIPlanetFocusManager != null) + " / !_resetButton.activeInHierarchy = " + (!_resetButton.activeInHierarchy));

        while (GalaxyExplorerManager.Instance.TransitionManager.InTransition)
        {
            yield return null;
        }

        Debug.Log("In OnSceneIsLoadedCoroutine after yield return null. (POIPlanetFocusManager != null) = " + (POIPlanetFocusManager != null) + " / !_resetButton.activeInHierarchy = " + (!_resetButton.activeInHierarchy));

        SetMenuVisibility(true);

        if (POIPlanetFocusManager != null && !_resetButton.activeInHierarchy)
        {
            Debug.Log("Enabling Reset button");

            // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system and the reset button should be visible
            _resetButton.SetActive(true);
            _backButton.transform.localPosition = new Vector3(_fullMenuVisibleBackButtonX, 0f, 0f);
        }
        // else if (POIPlanetFocusManager == null && _resetButton.activeInHierarchy)
        else if (POIPlanetFocusManager == null && _resetButton.activeInHierarchy)
        {
            Debug.Log("Disabling Reset button");

            // When the POIPlanetFocusManager isn't present in the currently loaded scenes, this means we're not in the solar system and the reset button shouldn't show up
            _resetButton.SetActive(false);
            _backButton.transform.localPosition = _defaultBackButtonLocalPosition;
        }
    }

    private void OnLoadNewScene()
    {
        if (_menuParent.activeInHierarchy)
        {
            SetMenuVisibility(false);
        }
    }

    public void OnToggleDesktopButtonVisibility()
    {
        _buttonParent.SetActive(!_buttonParent.activeSelf);
    }

    private void OnBackButtonNeedsToShow(bool show)
    {
        _backButton.SetActive(show);
    }

    public void OnAboutButtonPressed()
    {
        _aboutSlate.ButtonClicked();
    }

    public void OnBackButtonPressed()
    {
        GalaxyExplorerManager.Instance.TransitionManager.LoadPrevScene();
    }

    public void OnResetButtonPressed()
    {
        if (POIPlanetFocusManager)
        {
            _pOIPlanetFocusManager.ResetAllForseSolvers();
        }
        else
        {
            Debug.Log("No POIPlanetFocusManager found in currently loaded scenes");
        }
    }

    private void SetMenuVisibility(bool isVisible)
    {
        _menuParent.SetActive(isVisible);
        IsVisible = isVisible;
    }
}