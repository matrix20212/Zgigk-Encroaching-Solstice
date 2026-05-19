using UnityEngine;
using UnityEngine.Events;

public class ThreatManager : MonoBehaviour
{
    public static ThreatManager Instance;

    [Range(0f, 100f)]
    public float threatLevel = 0f;
    public float threatPerDay = 5f;

    public UnityEvent OnThreatChanged;
    public UnityEvent OnSolstice;

    void Awake() => Instance = this;

    public void IncreaseThreat()
    {
        threatLevel = Mathf.Min(100f, threatLevel + threatPerDay);
        OnThreatChanged?.Invoke();
    }

    public void TriggerSolstice()
    {
        Debug.Log("PRZESILENIE!");
        OnSolstice?.Invoke();
        // Tu wywo³aj spawn systemu przeciwników
        threatLevel = 10f;
        OnThreatChanged?.Invoke();
    }

    public Color GetThreatColor()
    {
        return Color.Lerp(Color.green, Color.red, threatLevel / 100f);
    }
}