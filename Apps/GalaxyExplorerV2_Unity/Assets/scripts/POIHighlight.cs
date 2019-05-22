using System.Collections;
using UnityEngine;

public class POIHighlight : MonoBehaviour
{
    [SerializeField]
    private float _startOffset = -1f;

    [SerializeField]
    private float _endOffset = 1f;

    [SerializeField]
    private float duration = 1f;

    [SerializeField]
    private float _delay = 0f;

    private Material _material;

    private void OnEnable()
    {
        if (_material == null)
        {
            _material = GetComponent<Renderer>().material;
        }

        StartLerpTextureOffset();
    }

    public void StartLerpTextureOffset()
    {
        StartCoroutine(LerpTextureOffset(duration));
    }

    private IEnumerator LerpTextureOffset(float duration)
    {
        _material.SetTextureOffset("_MainTex", new Vector2(_startOffset, 0));

        Vector2 newOffset = _material.GetTextureOffset("_MainTex");

        yield return new WaitForSeconds(_delay);

        while (duration > 0)
        {
            newOffset.x = Mathf.Lerp(_startOffset, _endOffset, duration);

            _material.SetTextureOffset("_MainTex", newOffset);

            duration -= Time.deltaTime;

            yield return null;
        }
    }
}