using UnityEngine;

public class HandMenuManager : MonoBehaviour
{
    [SerializeField]
    private HandMenu _handMenuLeft;

    [SerializeField]
    private HandMenu _handMenuRight;

    public bool MenuIsAlreadyShowing
    {
        get { return _handMenuLeft.MenuIsShowingOnThisHand || _handMenuRight.MenuIsShowingOnThisHand; }
    }
}