using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // Bu script eklendiği objede zorunlu olarak Button arar
public class PopupTrigger : MonoBehaviour
{
    [Header("Bu Butonun İçeriği")]
    public string itemTitle;
    
    [TextArea(3, 5)] // Inspector'da açıklamayı yazmak için geniş bir kutu oluşturur
    public string itemDescription;
    
    public Sprite itemImage;

    private Button myButton;

    private void Awake()
    {
        myButton = GetComponent<Button>();
        
        // Butona tıklandığında OnButtonClicked fonksiyonunu çalıştır
        myButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        // Eğer sahnede PopupManager varsa, elimizdeki verileri ona gönder
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(itemTitle, itemDescription, itemImage);
        }
        else
        {
            Debug.LogWarning("Sahnede PopupManager bulunamadı!");
        }
    }
}