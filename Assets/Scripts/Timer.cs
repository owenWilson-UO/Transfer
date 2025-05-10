using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Settings")]
    [SerializeField] public float currentTime { get; private set; }

    private bool startTimer;


    void Start()
    {
        timerText.text = "0.00";
        startTimer = false;
    }

    void Update()
    {
        if (startTimer)
        {
            currentTime += Time.deltaTime; 
        }
        timerText.text = currentTime.ToString("0.00");
    }

    public void StartTimer()
    {
        startTimer = true;
    }
    public void StopTimer()
    {
        startTimer = false;
    }
}
