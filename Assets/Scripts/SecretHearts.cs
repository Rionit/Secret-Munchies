using System;
using UnityEngine;

public class SecretHearts : MonoBehaviour
{
    public GameObject[] hearts;
    
    private void Start()
    {
        GameManager.Instance.onSecretHeartsChanged += SecretHeartsChanged;
    }

    private void SecretHeartsChanged(int amount)
    {
        int i = 1;
        foreach (GameObject heart in hearts)
        {
            heart.SetActive(i <= amount);
            i++;
        }
    }
}
