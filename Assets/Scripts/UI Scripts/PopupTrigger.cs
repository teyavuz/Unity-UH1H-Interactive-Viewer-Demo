using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PopupTrigger : MonoBehaviour
{
    [Header("Bu Butonun İçeriği")]
    public string itemTitle;
    
    [TextArea(3, 10)] // Listeler uzun olabileceği için kutuyu biraz büyüttüm
    public string itemDescription;
    
    //public Sprite itemImage;

    [Header("Metin Ayarları")]
    [Tooltip("Bunu işaretlersen, açıklamaya yazdığın her satır otomatik olarak maddeli listeye çevrilir.")]
    public bool isListText = false;

    private Button myButton;

    private void Awake()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnButtonClicked);
    }

private void OnButtonClicked()
    {
        if (PopupManager.Instance != null)
        {
            string finalDescription = itemDescription;

            // Eğer bu butonun metni liste olacaksa, arka planda dinamik olarak TMP formatına çevir
            if (isListText && !string.IsNullOrEmpty(itemDescription))
            {
                string[] lines = itemDescription.Split('\n');
                finalDescription = ""; // Eski metni sıfırla, yeniden inşa edeceğiz
                
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // TextMeshPro için kusursuz hizalamalı (Hanging Indent) madde imi kodu
                        // margin-left: paragrafı sağa iter, indent: sadece ilk satırı (noktayı) sola çeker
                        finalDescription += $"<margin-left=1.5em><indent=-1.5em>• </indent>{line.Trim()}</margin>\n";
                    }
                }
            }

            //PopupManager.Instance.ShowPopup(itemTitle, finalDescription, itemImage);
            PopupManager.Instance.ShowPopup(itemTitle, finalDescription);
        }
        else
        {
            Debug.LogWarning("Sahnede PopupManager bulunamadı!");
        }
    }
}