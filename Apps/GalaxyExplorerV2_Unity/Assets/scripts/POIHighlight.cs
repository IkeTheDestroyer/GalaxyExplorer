using System.Collections;
using UnityEngine;

public class POIHighlight : MonoBehaviour
{
    [SerializeField]
    private float _startOffset = -0.7f;

    [SerializeField]
    private float _endOffset = 0.7f;

    [SerializeField]
    private float duration = 1f;

    private Material _material;

    private void Start()
    {
        _material = GetComponent<Renderer>().material;

        StartLerpTextureOffset();
    }

    private void StartLerpTextureOffset()
    {
        _material.SetTextureOffset("_MainTex", new Vector2(_startOffset, 0));

        StartCoroutine(LerpTextureOffset(duration));
    }

    private IEnumerator LerpTextureOffset(float duration)
    {
        float totalDuration = duration;
        Vector2 newOffset = _material.GetTextureOffset("_MainTex");

        while (duration > 0)
        {
            newOffset.x = Mathf.Lerp(_startOffset, _endOffset, duration);

            _material.SetTextureOffset("_MainTex", newOffset);

            duration -= Time.deltaTime;

            yield return null;
        }
    }
}