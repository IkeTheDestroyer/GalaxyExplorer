using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlanetOffsetScaleController : MonoBehaviour
{
    private Animator _animator;
    private static readonly int Editing = Animator.StringToHash("Editing");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void SetEditBool(bool value)
    {
        _animator.SetBool(Editing, value);
    }

    public void SetToEdit()
    {
        SetEditBool(true);
    }

    public void SetToOrbit()
    {
        SetEditBool(false);
    }
}
