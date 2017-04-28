using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class MenuBehavior : MonoBehaviour
{
    private const int c_defaultDistance = 3;                                // default distance from a player
    private const float c_defaultButtonTimeout = 5;                         // default button click timeout in seconds
    private readonly string[] c_keyPhrases = { "Show menu", "Hide menu" };  // voice commands the menu can process

    public GameObject cubePrefab;
    public GameObject spherePrefab;
    public GameObject canvas;
    public Button sphereButton;
    public Button cubeButton;

    private float m_sphereButtonTimeout;
    private float m_cubeButtonTimeout;

    private KeywordRecognizer m_keywordRecognizer;          // recognizer listens for voice commands
    private bool m_isVoiceCmdActive = true;                        // if true the menu can process voice commands TODO:

    void Start()
    {
        if ((null == cubePrefab) || (null == spherePrefab) || (null == canvas) || (null == sphereButton) || (null == cubeButton))
        {
            Debug.LogError("Menu can not be initialized. Check settings in inspector.");
            Destroy(this);
            return;
        }

        SurfaceScanManager.Instance.OnScanCompleted += MyOnPlaySpaceScanCompleted;
        m_keywordRecognizer = new KeywordRecognizer(c_keyPhrases);
        m_keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        m_keywordRecognizer.Start();
    }

    private void Update()
    {
        MyCheckButtonsTimeouts();
    }

    private void OnDestroy()
    {
        if (null != m_keywordRecognizer)
        {
            m_keywordRecognizer.Stop();
            m_keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;
            m_keywordRecognizer.Dispose();
            m_keywordRecognizer = null;
        }
    }

    public void CubeButtonClick()
    {
        cubeButton.interactable = false;
        m_cubeButtonTimeout = Time.time + c_defaultButtonTimeout;
        Instantiate(cubePrefab, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity);
    }

    public void SphereButtonClick()
    {
        sphereButton.interactable = false;
        m_sphereButtonTimeout = Time.time + c_defaultButtonTimeout;
        Instantiate(spherePrefab, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity);
    }

    /// <summary>
    /// Places the menu on a default distance in front of a player and activates it.
    /// </summary>
    private void MyAppearBeforePlayer()
    {
        Transform cameraTransform = Camera.main.transform;
        transform.position = cameraTransform.position + (cameraTransform.forward * c_defaultDistance);
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
        canvas.SetActive(true);
    }

    /// <summary>
    /// Hides the menu.
    /// </summary>
    private void MyHide()
    {
        canvas.SetActive(false);
    }

    /// <summary>
    /// Checks if buttons' timeouts have elapsed and re-enables buttons.
    /// </summary>
    private void MyCheckButtonsTimeouts()
    {
        if (m_sphereButtonTimeout > 0)
        {
            if (Time.time > m_sphereButtonTimeout)
            {
                m_sphereButtonTimeout = 0;
                sphereButton.interactable = true;
            }
        }
        if (m_cubeButtonTimeout > 0)
        {
            if (Time.time > m_cubeButtonTimeout)
            {
                m_cubeButtonTimeout = 0;
                cubeButton.interactable = true;
            }
        }
    }

    private void MyOnPlaySpaceScanCompleted()
    {
        m_isVoiceCmdActive = true;
        MyAppearBeforePlayer();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (m_isVoiceCmdActive)
        {
            if (c_keyPhrases[0] == args.text)
                MyAppearBeforePlayer();
            else if (c_keyPhrases[1] == args.text)
                MyHide();
        }
    }
}
