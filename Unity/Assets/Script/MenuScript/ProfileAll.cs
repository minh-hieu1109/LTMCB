using UnityEngine;

public class ProfileAll : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public GameObject MenuGame;
    public void OnOpenMenuGame()
    {
        MenuGame.SetActive(true);
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
