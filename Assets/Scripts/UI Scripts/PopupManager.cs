using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullandığımız için bu kütüphane şart

public class PopupManager : MonoBehaviour
{
    // Singleton yapısı: Diğer scriptlerin bu yöneticiye kolayca ulaşmasını sağlar
    public static PopupManager Instance { get; private set; }

    [Header("UI Elementleri (Prefab İçindekiler)")]
    public GameObject popupPanel;       // Açılıp kapanacak ana panel
    public TextMeshProUGUI titleText;   // Başlık metni
    public TextMeshProUGUI descText;    // Açıklama metni
    //public Image productImage;          // Ürün görseli

    private GameManager.GameState previousState; // Popup açılmadan önceki state

    private void Awake()
    {
        // Singleton Kurulumu
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Oyun başladığında panelin kapalı olduğundan emin olalım
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    // Butonların çağırıp veri göndereceği ana fonksiyon
    public void ShowPopup(string title, string description/*, Sprite image*/)
    {
        // Önceki state'i sakla ve InspectPopup state'ine geç
        previousState = GameManager.Instance.CurrentState;
        GameManager.Instance.SetState(GameManager.GameState.InspectPopup);

        titleText.text = title;
        descText.text = description;

        // Eğer butona bir görsel atandıysa göster, atanmadıysa Image'i gizle
        //if (image != null)
        //{
            //productImage.sprite = image;
            //productImage.gameObject.SetActive(true);
        //}
        //else
        //{
            //productImage.gameObject.SetActive(false);
        //}

        // Verileri doldurduktan sonra paneli görünür yap
        popupPanel.SetActive(true);
    }

    // Popup'ı kapatmak için kullanılacak fonksiyon (Çarpı butonuna verebilirsin)
    public void ClosePopup()
    {
        popupPanel.SetActive(false);

        // Önceki state'e dön
        GameManager.Instance.SetState(previousState);
    }
}