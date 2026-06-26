using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Controla un Canvas en World Space con:
/// - Diapositivas: cada interacción pasa a la siguiente.
/// - La última diapositiva vuelve a la primera.
/// - Una imagen animada independiente que cambia de frame cada X segundos.
/// 
/// Uso principal:
/// 1. Pon este script en el objeto interactuable o en un objeto controlador.
/// 2. Asigna Slide Image y las Slide Sprites.
/// 3. Asigna Animated Image y los Animation Frames.
/// 4. Desde tu sistema de interacción llama a NextSlide().
/// </summary>
[DisallowMultipleComponent]
public class WorldSpaceSlideShowInteractable : MonoBehaviour
{
    [Header("Canvas World Space")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private bool forceWorldSpaceCanvas = false;
    [SerializeField] private Camera eventCamera;

    [Header("Diapositivas")]
    [SerializeField] private Image slideImage;
    [SerializeField] private Sprite[] slideSprites;
    [SerializeField] private int startSlideIndex = 0;

    [Header("Imagen animada")]
    [SerializeField] private Image animatedImage;
    [SerializeField] private Sprite[] animationFrames;

    [Tooltip("Tiempo entre frames en segundos. 0.75 = 750 ms.")]
    [SerializeField] private float animationFrameTime = 0.75f;

    [SerializeField] private bool playAnimationOnStart = true;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Interacción opcional")]
    [Tooltip("Solo para pruebas rápidas con OnMouseDown. Para tu juego usa NextSlide() desde tu sistema de interacción.")]
    [SerializeField] private bool allowMouseClickTest = false;

    [Header("Eventos")]
    public UnityEvent<int> onSlideChanged;
    public UnityEvent onWrappedToFirstSlide;

    private int currentSlideIndex;
    private int currentAnimationFrameIndex;
    private float animationTimer;
    private bool animationPlaying;

    public int CurrentSlideIndex => currentSlideIndex;
    public int SlideCount => slideSprites != null ? slideSprites.Length : 0;

    private void Awake()
    {
        SetupCanvasIfNeeded();

        currentSlideIndex = GetSafeSlideIndex(startSlideIndex);
        currentAnimationFrameIndex = 0;
    }

    private void Start()
    {
        ApplySlide(false);
        ApplyAnimationFrame();

        animationPlaying = playAnimationOnStart;
    }

    private void Update()
    {
        UpdateAnimatedImage();
    }

    /// <summary>
    /// Método principal para conectar en tu sistema de interacción.
    /// Cada llamada avanza una diapositiva.
    /// </summary>
    public void NextSlide()
    {
        if (slideSprites == null || slideSprites.Length == 0)
            return;

        currentSlideIndex++;

        bool wrapped = false;

        if (currentSlideIndex >= slideSprites.Length)
        {
            currentSlideIndex = 0;
            wrapped = true;
        }

        ApplySlide(true);

        if (wrapped)
            onWrappedToFirstSlide?.Invoke();
    }

    public void PreviousSlide()
    {
        if (slideSprites == null || slideSprites.Length == 0)
            return;

        currentSlideIndex--;

        if (currentSlideIndex < 0)
            currentSlideIndex = slideSprites.Length - 1;

        ApplySlide(true);
    }

    public void SetSlide(int index)
    {
        if (slideSprites == null || slideSprites.Length == 0)
            return;

        currentSlideIndex = Mathf.Clamp(index, 0, slideSprites.Length - 1);
        ApplySlide(true);
    }

    public void RestartSlides()
    {
        currentSlideIndex = 0;
        ApplySlide(true);
    }

    public void PlayAnimatedImage()
    {
        animationPlaying = true;
    }

    public void PauseAnimatedImage()
    {
        animationPlaying = false;
    }

    public void RestartAnimatedImage()
    {
        currentAnimationFrameIndex = 0;
        animationTimer = 0f;
        ApplyAnimationFrame();
    }

    /// <summary>
    /// Compatibilidad con sistemas de interacción que llamen a Interact() sin parámetros.
    /// </summary>
    public void Interact()
    {
        NextSlide();
    }

    /// <summary>
    /// Compatibilidad con sistemas de interacción que llamen a Interact(GameObject player).
    /// </summary>
    public void Interact(GameObject player)
    {
        NextSlide();
    }

    /// <summary>
    /// Compatibilidad con sistemas de interacción que llamen a Interact(Transform player).
    /// </summary>
    public void Interact(Transform player)
    {
        NextSlide();
    }

    private void UpdateAnimatedImage()
    {
        if (!animationPlaying)
            return;

        if (animatedImage == null)
            return;

        if (animationFrames == null || animationFrames.Length == 0)
            return;

        if (animationFrameTime <= 0f)
            return;

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        animationTimer += deltaTime;

        while (animationTimer >= animationFrameTime)
        {
            animationTimer -= animationFrameTime;
            currentAnimationFrameIndex++;

            if (currentAnimationFrameIndex >= animationFrames.Length)
                currentAnimationFrameIndex = 0;

            ApplyAnimationFrame();
        }
    }

    private void ApplySlide(bool notify)
    {
        if (slideImage == null)
            return;

        if (slideSprites == null || slideSprites.Length == 0)
        {
            slideImage.sprite = null;
            return;
        }

        currentSlideIndex = GetSafeSlideIndex(currentSlideIndex);
        slideImage.sprite = slideSprites[currentSlideIndex];
        slideImage.enabled = slideImage.sprite != null;

        if (notify)
            onSlideChanged?.Invoke(currentSlideIndex);
    }

    private void ApplyAnimationFrame()
    {
        if (animatedImage == null)
            return;

        if (animationFrames == null || animationFrames.Length == 0)
        {
            animatedImage.sprite = null;
            return;
        }

        currentAnimationFrameIndex = Mathf.Clamp(currentAnimationFrameIndex, 0, animationFrames.Length - 1);
        animatedImage.sprite = animationFrames[currentAnimationFrameIndex];
        animatedImage.enabled = animatedImage.sprite != null;
    }

    private int GetSafeSlideIndex(int index)
    {
        if (slideSprites == null || slideSprites.Length == 0)
            return 0;

        return Mathf.Clamp(index, 0, slideSprites.Length - 1);
    }

    private void SetupCanvasIfNeeded()
    {
        if (targetCanvas == null)
            targetCanvas = GetComponentInChildren<Canvas>(true);

        if (targetCanvas == null)
            return;

        if (forceWorldSpaceCanvas)
            targetCanvas.renderMode = RenderMode.WorldSpace;

        if (targetCanvas.renderMode == RenderMode.WorldSpace && targetCanvas.worldCamera == null)
            targetCanvas.worldCamera = eventCamera != null ? eventCamera : Camera.main;

        GraphicRaycaster raycaster = targetCanvas.GetComponent<GraphicRaycaster>();

        if (raycaster != null)
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
    }

    private void OnMouseDown()
    {
        if (!allowMouseClickTest)
            return;

        NextSlide();
    }
}
