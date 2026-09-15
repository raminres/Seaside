using UnityEngine;
using UnityEngine.InputSystem;

public class TextureChanger : MonoBehaviour
{
    [SerializeField] private Renderer carpetRenderer;
    [SerializeField] private Texture[] textures;
    private int currentTextureIndex;
    private PlayerInput playerInput;
    private InputAction changeTextureAction;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        
        playerInput = GetComponent<PlayerInput>();
        changeTextureAction = playerInput != null ? playerInput.actions.FindAction("ChangeTexture", false) : null;
        propertyBlock = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        
        if (changeTextureAction != null) changeTextureAction.performed += OnChangeTexture;
    }

    void OnDisable()
    {
        if (changeTextureAction != null) changeTextureAction.performed -= OnChangeTexture;
    }

    public void OnChangeTexture(InputAction.CallbackContext context)
    {
        if (carpetRenderer == null || textures == null || textures.Length == 0) return;

        currentTextureIndex = (currentTextureIndex + 1) % textures.Length;
        Material material = carpetRenderer.sharedMaterial;
        if (material == null) return;

        int texturePropertyId = material.HasProperty("_BaseMap")
            ? Shader.PropertyToID("_BaseMap")
            : Shader.PropertyToID("_MainTex");
        propertyBlock ??= new MaterialPropertyBlock();
        carpetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(texturePropertyId, textures[currentTextureIndex]);
        carpetRenderer.SetPropertyBlock(propertyBlock);
    }
}
