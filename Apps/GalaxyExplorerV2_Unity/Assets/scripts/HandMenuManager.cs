using UnityEngine;

public class HandMenuManager : MonoBehaviour
{
    [SerializeField]
    private HandMenu _handMenuLeft;

    [SerializeField]
    private HandMenu _handMenuRight;

    public bool IsAMenuVisible
    {
        get { return _handMenuLeft.IsVisible || _handMenuRight.IsVisible; }
    }
}