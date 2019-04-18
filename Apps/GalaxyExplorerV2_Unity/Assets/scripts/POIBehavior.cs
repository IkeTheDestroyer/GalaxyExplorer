using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using MRS.Layers;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class POIBehavior : MonoBehaviour//, IMixedRealityPointerHandler
{
    [SerializeField] private List<Renderer> objectsToFade;
    [SerializeField] private List<TextMeshPro> textsToFade;
    [SerializeField] private float alphaColor = .2f;
    [SerializeField] private GameObject pressableButton;
    
    private BoxCollider boxCollider;
    private GameObject camera;
    private Transform[] corners;
    private bool fading;
    private bool forceFading;
    private List<Material> materialsToFade;
    
    RaycastHit[] raycastResults = new RaycastHit[1];
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        camera = Camera.main.gameObject;
        corners = new Transform[5];
        Vector3[] verts = new Vector3[5]; 
        verts[0] = transform.TransformPoint(boxCollider.center);
        verts[1] = transform.TransformPoint(boxCollider.center + (new Vector3(boxCollider.size.x, boxCollider.size.y, 0) * 0.4f));
        verts[2] = transform.TransformPoint(boxCollider.center + (new Vector3(boxCollider.size.x, -boxCollider.size.y, 0) * 0.4f));
        verts[3] = transform.TransformPoint(boxCollider.center + (new Vector3(-boxCollider.size.x, boxCollider.size.y, 0) * 0.4f));
        verts[4] = transform.TransformPoint(boxCollider.center + (new Vector3(-boxCollider.size.x, -boxCollider.size.y, 0) * 0.4f));

        for (var i = 0; i < verts.Length; i++)
        {
            var go = new GameObject($"corner {i}");
            go.transform.position = verts[i];
            go.transform.SetParent(transform, true);
            corners[i] = go.transform;
        }
        
        var anchor = new GameObject("Anchor");
        anchor.transform.SetParent(transform.parent, false);
        anchor.transform.localPosition = transform.localPosition;

        materialsToFade = new List<Material>();

        foreach (var fadingObject in objectsToFade)
        {
            materialsToFade.Add(fadingObject.material);
        }
        
        foreach (var fadingText in textsToFade)
        {
            fadingText.color = Color.clear;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (forceFading)
        {
            return;
        }
        var allPointsVisible = IsPointVisible(corners[0].position);;
        allPointsVisible = allPointsVisible && IsPointVisible(corners[1].position);
        allPointsVisible = allPointsVisible && IsPointVisible(corners[2].position);
        allPointsVisible = allPointsVisible && IsPointVisible(corners[3].position);
        allPointsVisible = allPointsVisible && IsPointVisible(corners[4].position);
        Fade(allPointsVisible);
    }

    private bool IsPointVisible(Vector3 position)
    {
        var layerMask = 1 <<LayerMask.NameToLayer("POI");
        var direction = (position - camera.transform.position).normalized;
        Debug.DrawRay(camera.transform.position, (position - camera.transform.position)*2, Color.green);
        RaycastHit hit;
        var isVisible = false;
        if(Physics.Raycast(camera.transform.position, direction, out hit, float.PositiveInfinity, layerMask ))
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    return true;
                }
            }
        }
       return false;
    }


    public void Fade(bool fadeIn, bool force = false)
    {
        forceFading = force;
        StartCoroutine(FadeRoutine(fadeIn));
        pressableButton.SetActive(fadeIn);

    }

    private IEnumerator FadeRoutine(bool fadeIn)
    {
        var fadingColor = fadeIn ? Color.white:  new Color(1,1,1,alphaColor);
        foreach (var material in materialsToFade)
        {
            material.color = fadingColor;
        }
        
        foreach (var fadingText in textsToFade)
        {
            fadingText.color = fadingColor;
        }
        yield return null;
//        var overTime = 1f;
//        var timeSoFar = 0f;
//        while (timeSoFar < overTime)
//        {
//            text.color = new Color(text.color.r, text.color.g, text.color.b, timeSoFar/overTime);
//            yield return null;
//            timeSoFar += Time.deltaTime;
//        }
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        Debug.Log($"!!^!! on pointer up");
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        Debug.Log($"!!^!! on pointer down");
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        Debug.Log($"!!^!! on pointer clicked");
    }
}
