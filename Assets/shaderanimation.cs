using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shaderanimation : MonoBehaviour
{ // Store the SpriteRenderer and Material as private variables
    private SpriteRenderer spriteRenderer;
    private Material material;

    // Initialize the SpriteRenderer and Material in the Start method
    void Start()
    {
        // Get the sprite renderer and its material
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;

        // The texel size for a 256x256 texture
        Vector4 texelSize = new Vector4(1.0f / 256, 1.0f / 256, 256, 256);

        // Assign the texel size to the material
        material.SetVector("_MainTex_TexelSize", texelSize);
    }
}
