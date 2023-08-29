using UnityEngine;

public class PrintAndHide : MonoBehaviour
{
    private int i = 3;
    public Renderer rend;
    private int randomInt;
    void Start()
    {
        
        rend = GetComponent<Renderer>();
        randomInt = Random.Range(150, 251);
    }

    void Update()
    {
        i++; 
        Debug.Log(gameObject.name + ":" + i);  
        

        if (gameObject.tag == "Red" && i == 100)
        {
            gameObject.SetActive(false);
        }

        if (gameObject.tag == "Blue" && i == randomInt)
        {
            rend.enabled = false;
        }
    }
}

