using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Game States
    public enum GameState
    {
        //MainMenu,
        Transition,
        Freelook,
        Inspect,
        InspectGauge,
        InspectHeadUnit,
        InspectMidUnit,
        InspectPopup,
    }
    #endregion

    #region Inspector Fields

    [Header("State")]
    [SerializeField] private GameState initialState = GameState.Freelook;

    [ReadOnly]
    [SerializeField] private GameState currentState = GameState.Freelook;

    public GameState CurrentState => currentState;

    [Header("Scene References")]
    [SerializeField] private OrbitCameraController orbitCamera;

    #endregion


    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetState(initialState, force: true);
    }

    #endregion

    

    public void SetState(GameState newState, bool force = false)
    {
        if (!force && currentState == newState) return;

        OnExitState(currentState);
        currentState = newState;
        OnEnterState(currentState);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }



    #region State Transitions
    // State giriş çıkışlarında yapılacak işlemler

    private void OnEnterState(GameState state)
    {
        Debug.Log("State changed to: " + currentState);
    switch (state)
        {
        case GameState.Freelook:
            if (orbitCamera != null)
                orbitCamera.enabled = true;
            break;
        case GameState.InspectPopup:
            if (orbitCamera != null)
                orbitCamera.enabled = false;
            break;
        }
    }

    private void OnExitState(GameState state)
    {
    switch (state)
        {
        case GameState.Freelook:
            if (orbitCamera != null)
                orbitCamera.enabled = false;
            break;
        case GameState.InspectPopup:
            if (orbitCamera != null)
                orbitCamera.enabled = true;
            break;
        }
    }
    #endregion
}
