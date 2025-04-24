using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

public class EmissionModifier : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    [SerializeField, OnChanged(nameof(OnIntensityChange))]
    public Color color;

    [SerializeField, OnChanged(nameof(OnIntensityChange))]
    public float intensity;

    private string PropertyName {
        get {
            return "_EmissionColor"; // URP
            // return "_EmissiveColor"; // HDRP
        }
    }

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        color = meshRenderer.material.GetColor(PropertyName);
   }
    
    void Update()
    {
        // float factor = Mathf.Pow(2, intensity);
        // Color color = new Color(c.r* factor, c.g* factor, c.b* factor);
        // material.SetColor(property, color);
    }

    public void OnIntensityChange() 
    {
        if (meshRenderer == null)
            ChangeColor(GetComponent<MeshRenderer>(), color, intensity);    
        else ChangeColor(meshRenderer, color, intensity);
    }

    private void ChangeColor(MeshRenderer meshRenderer, Color color, float intensity)
    {
        foreach (var material in meshRenderer.materials)
            if (material.IsKeywordEnabled("_EMISSION"))
                material.SetColor(PropertyName, color * intensity);
    }
}

#endif
