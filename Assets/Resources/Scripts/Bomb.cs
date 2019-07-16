using TMPro;
using UnityEngine;

public class Bomb : Hex
{
    public static Bomb instance = null;     // Singleton instance
    public int counter = 10;                // Bomb counter. When reaches zero, triggers game over state.
    private TextMeshProUGUI counterText;    // Text component to show the bomb counter

    //Singleton
    void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.GetComponent<Hex>().Blow();
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {   
        // Text initialization
        counterText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        counterText.text = counter.ToString();
        // Select a random color from pallette for the object.
        int i = Random.Range(0, ColorPallette.colors.Length);
        color = i;
        GetComponent<SpriteRenderer>().color = ColorPallette.colors[i];
    }

    // To count down and to trigger the game over when counter reaches zero.
    public void CountDown()
    {
        counter--;
        counterText.text = counter.ToString();
        if (counter == 0)
        {
            GameManager.instance.GameOver();
        }
    }
    // Destructor
    public new void Blow()
    {
        // Parse the name to get indexes of the object at the grid array
        int[] gridArrIndex = new int[2];
        gridArrIndex[0] = (int)char.GetNumericValue(gameObject.name[gameObject.name.Length - 2]);
        gridArrIndex[1] = (int)char.GetNumericValue(gameObject.name[gameObject.name.Length - 1]);
        // And null out the index that this object was on.
        GridManager.instance.gridArrray[gridArrIndex[0], gridArrIndex[1]] = null;
        // Null the singleton instance
        instance = null;
        // Update the score and destroy the object. 
        GameManager.instance.AddScore(5);
        Destroy(gameObject);
    }
}
