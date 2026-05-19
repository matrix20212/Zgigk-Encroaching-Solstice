using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingButton : MonoBehaviour
{
    public BuildingData buildingData;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);

        var label = GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = $"{buildingData.buildingName}\n🪵{buildingData.woodCost}";
    }

    void OnClick()
    {
        BuildingManager.Instance.StartBuilding(buildingData);
    }
}