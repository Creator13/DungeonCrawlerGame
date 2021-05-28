using TMPro;
using UnityEngine;

namespace UI {
public class ConnectionItem : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;

    private int id;

    public int ID {
        get => id;
        set {
            id = value;
            text.text = $"ID: {id}";
        }
    }
}
}
