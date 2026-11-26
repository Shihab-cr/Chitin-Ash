using System;
using UnityEngine;
using UnityEngine.Events;


public class PlayerScrapesHandler : MonoBehaviour
{
    private int playerScrapes = 0;
    public UnityEvent<int> OnScrapeChange;
    [SerializeField] private GameObject playerCorpse;
    void Start()
    {
        playerScrapes = PlayerDataModel.LoadScrapesAmount();
    }
    public void AddScrapesToPlayer(int scrapes)
    {
        playerScrapes += scrapes;
        PlayerDataModel.SaveScrapesAmount(playerScrapes);
        OnScrapeChange?.Invoke(playerScrapes);
    }
    public void DropAllPlayerScrapes()
    {
        //instantiate the playerCorpse and then add the scrapes to it
        playerScrapes = 0;
        PlayerDataModel.SaveScrapesAmount(playerScrapes);
        OnScrapeChange?.Invoke(playerScrapes);
    }
    public bool UseScrapes(int requiredScrapes)
    {
        if (playerScrapes >= requiredScrapes)
        {
            playerScrapes -= requiredScrapes;
            PlayerDataModel.SaveScrapesAmount(playerScrapes);
            OnScrapeChange?.Invoke(playerScrapes);
            return true;
        }
        else
        {
            return false;
        }
    }
}
