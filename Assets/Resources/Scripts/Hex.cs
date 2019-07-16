using UnityEngine;

public class Hex : MonoBehaviour
{
    public int color;   // Color index of hexagons, for comparison

    void Start()
    {
        // Select a random color from pallette for the object.
        int i = Random.Range(0, ColorPallette.colors.Length);
        color = i;
        GetComponent<SpriteRenderer>().color = ColorPallette.colors[i];
    }

    // To highlight the object
    public void Highlight(bool isOn)
    {
        float addition;

        if (isOn)
            addition = .1f;
        else
            addition = -.1f;

        Color c = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = new Color(c.r + addition, c.g + addition, c.b + addition);

    }

    //Destructor
    public void Blow()
    {
        // Parse the name to get indexes at grid array
        int[] gridArrIndex = new int[2];
        gridArrIndex[0] = (int)char.GetNumericValue(gameObject.name[gameObject.name.Length - 2]);
        gridArrIndex[1] = (int)char.GetNumericValue(gameObject.name[gameObject.name.Length - 1]);
        // And null out the index that this object was on.
        GridManager.instance.gridArrray[gridArrIndex[0], gridArrIndex[1]] = null;
        // Update the score and destroy the object. 
        GameManager.instance.AddScore(5);
        Destroy(gameObject);
    }
}

/*   public void SlideToOwnPlace()
{
    StartCoroutine("SlideTo");
}
IEnumerator SlideTo()
{
    int xPos = (int)char.GetNumericValue(gameObject.name[gameObject.name.Length - 2]);
    int yPos = (int)char.GetNumericValue(gameObject.name[gameObject.name.Length - 1]);

    Vector3 place = new Vector3(xPos * GridManager.instance.hexWidth * 0.75f - GridManager.instance.rightPadding,
                                yPos * GridManager.instance.hexHeight - GridManager.instance.topPadding - (xPos % 2 == 0 ? GridManager.instance.hexHeight / 2 : 0),1);

    while (transform.position != place) {
        transform.position = Vector3.Lerp(transform.position, place,Time.deltaTime*25);
        yield return null;
    }
    if(yPos!= (int)char.GetNumericValue(gameObject.name[gameObject.name.Length - 1]))
    {
        StartCoroutine("SlideTo");
    }
}*/
