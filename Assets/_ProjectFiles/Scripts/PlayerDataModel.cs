using UnityEngine;

public class PlayerDataModel
{
    private static string scrapes = "ScrapesAmount";
    private static string health = "PlayerHealth";
    private static string positionX = "positionX";
    private static string positionY = "positionY";
    private static string positionZ = "positionZ";
    
    public static void SaveScrapesAmount(int scrapesAmount)
    {
        PlayerPrefs.SetInt(scrapes, scrapesAmount);
    }
    public static int LoadScrapesAmount()
    {
        return PlayerPrefs.GetInt(scrapes, 0);
    }
    public static void SaveHealth(float healthAmount)
    {
        PlayerPrefs.SetFloat(health, healthAmount);
    }
    public static float LoadHealth() {
        return PlayerPrefs.GetFloat(health, 100);
    }
    public static void SavePlayerPosition(float x, float y, float z)
    {
        PlayerPrefs.SetFloat(positionX, x);
        PlayerPrefs.SetFloat(positionY, y);
        PlayerPrefs.SetFloat(positionZ, z);
    }
    public static Vector3 LoadPlayerPosition()
    {
        return new Vector3(PlayerPrefs.GetFloat(positionX,0),PlayerPrefs.GetFloat(positionY,0),PlayerPrefs.GetFloat(positionZ,0));
    }
}
