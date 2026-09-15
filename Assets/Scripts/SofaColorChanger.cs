using UnityEngine;
using UnityEngine.InputSystem;

public class SofaColorChanger : MonoBehaviour
{
    [SerializeField] private Renderer sofaRenderer;
    [SerializeField] private Color[] colors;
    private int currentColorIndex;
    private PlayerInput playerInput;
    private InputAction changeColorAction;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        changeColorAction = playerInput != null ? playerInput.actions.FindAction("ChangeColor", false) : null;
        propertyBlock = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        if (changeColorAction != null) changeColorAction.performed += OnChangeColor;
    }

    void OnDisable()
    {
        if (changeColorAction != null) changeColorAction.performed -= OnChangeColor;
    }

    public void OnChangeColor(InputAction.CallbackContext context)
    {
        if (sofaRenderer == null || colors == null || colors.Length == 0) return;

        currentColorIndex = (currentColorIndex + 1) % colors.Length;
        Material material = sofaRenderer.sharedMaterial;
        if (material == null) return;

        int colorPropertyId = material.HasProperty("_BaseColor")
            ? Shader.PropertyToID("_BaseColor")
            : Shader.PropertyToID("_Color");
        propertyBlock ??= new MaterialPropertyBlock();
        sofaRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(colorPropertyId, colors[currentColorIndex]);
        sofaRenderer.SetPropertyBlock(propertyBlock);
    }
}
