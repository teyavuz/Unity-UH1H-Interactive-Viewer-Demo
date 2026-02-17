using UnityEngine;
using DG.Tweening;

public class LeftMenuAnimator : MonoBehaviour
{
    [Header("References (Layers)")]
    [SerializeField] private RectTransform imgBG;
    [SerializeField] private RectTransform imgPurple;

    [Header("Buttons Container (has LayoutGroup inside)")]
    [SerializeField] private RectTransform buttonsRoot;

    [Header("Open Button (auto hide/show)")]
    [SerializeField] private GameObject openButtonGO; // sadece bunu yöneteceğiz

    [Header("Open/Close Positions")]
    [SerializeField] private float openX = 0f;

    [Tooltip("Kapalı konum için sola kaçış miktarı (panel genişliğinden büyük olsun).")]
    [SerializeField] private float closedOffsetX = 800f;

    [Header("Timings (seconds)")]
    [SerializeField] private float bgDuration = 0.40f;
    [SerializeField] private float purpleDuration = 0.25f; // mor daha hızlı
    [SerializeField] private float buttonsDuration = 0.28f;
    [SerializeField] private float layerGap = 0.04f;

    [Header("Smoothing Curve")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Startup")]
    [SerializeField] private bool startClosed = true;

    private bool isOpen;
    private Sequence seq;

    private void Awake()
    {
        if (startClosed)
        {
            SnapClosed();
            isOpen = false;
        }
        else
        {
            SnapOpen();
            isOpen = true;
        }

        UpdateOpenButtonVisibility();
    }

    private void Start()
    {
        // Layout/Canvas hesapları Start'ta oturabiliyor; garanti olsun diye tekrar snap.
        if (startClosed) SnapClosed();
        else SnapOpen();

        UpdateOpenButtonVisibility();
    }

    public void OpenMenuFromButton()
    {
        if (isOpen) return;
        OpenMenu();
        isOpen = true;
        UpdateOpenButtonVisibility();
    }

    public void CloseMenuFromButton()
    {
        if (!isOpen) return;
        CloseMenu();
        isOpen = false;
        UpdateOpenButtonVisibility();
    }

    private void UpdateOpenButtonVisibility()
    {
        if (openButtonGO != null)
            openButtonGO.SetActive(!isOpen); // menü açıkken açma butonu görünmesin
    }

    private void OpenMenu()
    {
        KillSeqIfNeeded();

        seq = DOTween.Sequence();

        seq.Append(MoveX(imgBG, openX, bgDuration));
        seq.AppendInterval(layerGap);

        seq.Append(MoveX(imgPurple, openX, purpleDuration));
        seq.AppendInterval(layerGap);

        seq.Append(MoveX(buttonsRoot, openX, buttonsDuration));
    }

    private void CloseMenu()
    {
        KillSeqIfNeeded();

        float closedX = GetClosedX();

        seq = DOTween.Sequence();

        // Kapanırken ters sıra
        seq.Append(MoveX(buttonsRoot, closedX, buttonsDuration));
        seq.AppendInterval(layerGap);

        seq.Append(MoveX(imgPurple, closedX, purpleDuration));
        seq.AppendInterval(layerGap);

        seq.Append(MoveX(imgBG, closedX, bgDuration));
    }

    private void SnapClosed()
    {
        float closedX = GetClosedX();

        if (imgBG != null) imgBG.anchoredPosition = new Vector2(closedX, imgBG.anchoredPosition.y);
        if (imgPurple != null) imgPurple.anchoredPosition = new Vector2(closedX, imgPurple.anchoredPosition.y);
        if (buttonsRoot != null) buttonsRoot.anchoredPosition = new Vector2(closedX, buttonsRoot.anchoredPosition.y);
    }

    private void SnapOpen()
    {
        if (imgBG != null) imgBG.anchoredPosition = new Vector2(openX, imgBG.anchoredPosition.y);
        if (imgPurple != null) imgPurple.anchoredPosition = new Vector2(openX, imgPurple.anchoredPosition.y);
        if (buttonsRoot != null) buttonsRoot.anchoredPosition = new Vector2(openX, buttonsRoot.anchoredPosition.y);
    }

    private float GetClosedX()
    {
        return openX - Mathf.Abs(closedOffsetX);
    }

    private Tweener MoveX(RectTransform rt, float x, float duration)
    {
        if (rt == null) return null;
        return rt.DOAnchorPosX(x, duration).SetEase(moveCurve);
    }

    private void KillSeqIfNeeded()
    {
        if (seq != null && seq.IsActive())
            seq.Kill();
    }
}
