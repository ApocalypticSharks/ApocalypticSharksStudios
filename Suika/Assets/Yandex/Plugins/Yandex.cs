using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Yandex : MonoBehaviour
{
    public static Yandex instance;
    //[SerializeField] private Text _playerName;
    //[SerializeField] private RawImage _playerPhoto;

    [DllImport("__Internal")]
    private static extern void GetPlayerDataExtern();

    [DllImport("__Internal")]
    private static extern void RateGameExtern();

    [DllImport("__Internal")]
    private static extern void SaveExtern(string data);

    [DllImport("__Internal")]
    private static extern void LoadExtern();

    [DllImport("__Internal")]
    private static extern void ShowAdsExtern();

    [DllImport("__Internal")]
    private static extern void ShowRewardedAdsExtern(int amount);

    [DllImport("__Internal")]
    private static extern string GetLangExtern();

    public void Awake()
    {
        if (instance == null)
            instance = this;
#if !UNITY_EDITOR
        //GetPlayerData();
#endif
    }

    //public void SetPlayerName(string name)
    //{
    //    _playerName.text = name;
    //}
    //public void SetPlayerPhoto(string url)
    //{
    //    StartCoroutine(DownloadImage(url));
    //}
    //IEnumerator DownloadImage(string mediaUrl)
    //{
    //    UnityWebRequest request = UnityWebRequestTexture.GetTexture(mediaUrl);
    //    yield return request.SendWebRequest();
    //    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    //        Debug.Log(request.error);
    //    else
    //        _playerPhoto.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    //}
    //public void GetPlayerData()
    //{
    //    GetPlayerDataExtern();
    //}

    public void RateGame()
    {
        RateGameExtern();
    }

    public void SaveData(SavedGameData dataToSave)
    {
        string jsonString = JsonUtility.ToJson(dataToSave);
        SaveExtern(jsonString);
    }
    public void LoadData(string data, ref SavedGameData savedGameData)
    { 
        savedGameData = JsonUtility.FromJson<SavedGameData>(data);
    }
    public void GetDataToLoad()
    {
        LoadExtern();
    }

    public string SelectedLanguage()
    {
        return GetLangExtern();
    }

    public void ShowAds() 
    {
        ShowAdsExtern();
    }
    public void ShowRewardedAds(int amount)
    {
        ShowRewardedAdsExtern(amount);
    }
}
