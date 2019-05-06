using UnityEngine;

[ExecuteInEditMode]
public class PlanetOffsetScaleController : MonoBehaviour
{
    public static readonly float TargetEditScaleCm = 30f;
    public static readonly float TargetOrbitScaleToCm = 0.0215517241f;//this assures a sun diameter of 30cm
    public static readonly float GlobalPlanetScaleFactor = 15f;
    
    public float PlanetDiameterInKilometer = 1000f;
    public bool UseGlobalPlanetScaleFactor = true;
    public float CustomPerPlanetScaleFactor = 1f;

    private void Update()
    {
        transform.localScale =
            Vector3.one * PlanetDiameterInKilometer * .001f * TargetOrbitScaleToCm *
            (UseGlobalPlanetScaleFactor
                ? GlobalPlanetScaleFactor
                : CustomPerPlanetScaleFactor);
    }
}
