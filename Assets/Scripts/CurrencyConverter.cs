using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CurrencyConverter : MonoBehaviour
{
    private const string _apiKey = "3c4876bb5b6fa7f0708c748265534879";
    private const string _baseURL = "http://api.currencylayer.com/live";
    private const string _sourceCurrency = "USD";
    private const string _targetCurrency = "EUR";

    [SerializeField] private TMP_Text _rateDisplay;
    [SerializeField] private TMP_Text _refreshTimerText;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private float _refreshIntervalSeconds = 300f;
    private float _timer;

    private void Start()
    {
        if (_refreshButton != null)
        {
            _refreshButton.onClick.AddListener(FetchRate);
        }
        
        StartCoroutine(AutoUpdateRates());
    }

    private void Update()
    {
        UpdateTimer();
    }

    private IEnumerator AutoUpdateRates()
    {
        while (true)
        {
            FetchRate();
            yield return new WaitForSeconds(_refreshIntervalSeconds);
        }
    }

    private void FetchRate()
    {
        string url = $"{_baseURL}?access_key={_apiKey}&source={_sourceCurrency}&currencies={_targetCurrency}";
        StartCoroutine(FetchData(url));
    }

    private IEnumerator FetchData(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessResponse(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error fetching rate: " + request.error);
                if (_rateDisplay != null)
                {
                    _rateDisplay.text = "Error fetching rate.";
                }
            }
        }
    }

    private void ProcessResponse(string jsonResponse)
    {
        try
        {
            CurrencyData data = JsonUtility.FromJson<CurrencyData>(jsonResponse);
            if (data.success)
            {
                float usdToEur = data.quotes.USDEUR;
                if (_rateDisplay != null)
                {
                    _rateDisplay.text = $"USD/EUR: {usdToEur}";
                    ResetTimer();
                }

                Debug.Log($"USD to EUR rate: {usdToEur}");
            }
            else
            {
                Debug.LogError("API call was not successful.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error processing response: " + ex.Message);
            if (_rateDisplay != null)
            {
                _rateDisplay.text = "Error processing data.";
            }
        }
    }

    private void ResetTimer()
    {
        _timer = _refreshIntervalSeconds;
        UpdateTimerDisplay();
    }

    private void UpdateTimer()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (_refreshTimerText != null)
        {
            int minutes = Mathf.FloorToInt(_timer / 60);
            int seconds = Mathf.FloorToInt(_timer % 60);
            _refreshTimerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }
}

[Serializable]
public class CurrencyData
{
    public bool success;
    public string terms;
    public string privacy;
    public long timestamp;
    public string source;
    public Quotes quotes;
}

[Serializable]
public class Quotes
{
    public float USDEUR;
}