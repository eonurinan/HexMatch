using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance = null;          // Singleton instance
    public int gridWidth, gridHeight;                   // Grid will be gridWidth x gridHeight
    private GameObject hexagonPrefab, bombPrefab;       // Prefabs
    public List<GameObject> selected;                   // Selected hexagons
    public GameObject parentOfSelected;                 // Selected hexagons will be the children of this gameobject, to distinguish easily
    public GameObject[,] gridArrray;                    // 2D array of the grid to store the hexagons
    public bool bombIsReady;                            // If true, the next instantiation will be the bomb instead of a hexagon.
    private float hexWidth, hexHeight,                  // Width and height of the hexagon objects
                rightPadding, topPadding;               // How the grid will be padded from right and top
    private bool falling;
    #region Unity Callbacks
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        bombPrefab = Resources.Load<GameObject>("Prefabs/Bomb");
        hexagonPrefab = Resources.Load<GameObject>("Prefabs/Hexagon");
        gridArrray = new GameObject[gridWidth, gridHeight];


        // Scale hexagons for fitting them to all screens
        // This was a temporary solution, I didn't have time to finish it properly
        float screenWidth = Camera.main.orthographicSize * Screen.width / Screen.height * 2;
        float hexScale = 1.2f * screenWidth / 71.2f;   //71.2 is 10% width of the original sprite

        hexagonPrefab.transform.localScale = new Vector3(hexScale, hexScale, 1);
        bombPrefab.transform.localScale = new Vector3(hexScale * 712/20, hexScale * 712 / 20, 1);
        //--------------------------------------------------------------

        hexWidth = hexagonPrefab.GetComponent<Renderer>().bounds.size.x;
        hexHeight = hexagonPrefab.GetComponent<Renderer>().bounds.size.y;
        

        // To align the grid at the center of the screen
        rightPadding = (Mathf.Floor(gridWidth / 2) * hexWidth * 1.5f
                        + (gridWidth % 2 == 0 ? hexWidth * 0.25f : hexWidth)) / 2 - hexWidth / 2;        // Half-width formula of full grid
        topPadding = ((gridHeight - 1.5f) * hexHeight) / 2;                                              // Half-height formula of full grid minus one hexagon
                                                                                                         // because one hex goes below the origin at the beginning 
    }

    private void Start()
    {
        parentOfSelected = GameObject.FindGameObjectWithTag("Grouper");
        selected = new List<GameObject>();
        bombIsReady = false;

    }
    #endregion

    // To fill the grid at the start
    public void FillGrid()
    {
        for (int i = 0; i < gridWidth; i++)
            for (int j = 0; j < gridHeight; j++)
            {
                GameObject hex = Instantiate(hexagonPrefab
                                            , new Vector2(i * hexWidth * 0.75f - rightPadding, j * hexHeight - topPadding - (i % 2 == 0 ? hexHeight / 2 : 0)) // Put hexagons at hexHeight/2 higher if grid numbers are even
                                            , Quaternion.identity);
                hex.name = "Hex" + i + j;
                gridArrray[i, j] = hex;
            }
    }

    // To rotate selected 3 hexegons in terms of rotating;
    // graphically (currently by swapping positions), in the grid array and in the scene, by changing names.
    public void RotateSelected(bool isClockwise)
    {                                                       // For rotating visually, tried parenting and rotating the parent
                                                            // or rotating the objects seperately around the midpoint,
                                                            // but they raised collision bugs, I gave up after 1 day struggle

        if (selected == null)
            return;

        if (gridHeight < 10 || gridWidth < 10)
        {

            /** ------- Swap the Hexagons Graphically -------  **/

            int[] indexes = isClockwise ? new int[] { 0, 1, 2 } : new int[] { 2, 1, 0 };

            Vector3 temp = new Vector3();
            temp = selected[indexes[0]].transform.position;

            selected[indexes[0]].transform.position = selected[indexes[1]].transform.position;
            selected[indexes[1]].transform.position = selected[indexes[2]].transform.position;
            selected[indexes[2]].transform.position = temp;

            /** -------  Get the Indexes of Selected Hexagons -------  **/
            /** -------  by Parsing Their Names. -------  **/
            /** -------  Name format: Hex + x Index + y Index -------  **/
            /** -------  Name example: "Hex05" -------  **/

            int[,] hexArrIndexes = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                string hexName = selected[indexes[i]].name;
                hexArrIndexes[i, 0] = (int)char.GetNumericValue(hexName[hexName.Length - 2]);              // Row
                hexArrIndexes[i, 1] = (int)char.GetNumericValue(hexName[hexName.Length - 1]);              // Column of ith element
            }

            /** -------  Swap the Hexagons the Array -------  **/

            GameObject tempHex = gridArrray[hexArrIndexes[2, 0], hexArrIndexes[2, 1]];
            gridArrray[hexArrIndexes[2, 0], hexArrIndexes[2, 1]] = gridArrray[hexArrIndexes[1, 0], hexArrIndexes[1, 1]];
            gridArrray[hexArrIndexes[1, 0], hexArrIndexes[1, 1]] = gridArrray[hexArrIndexes[0, 0], hexArrIndexes[0, 1]];
            gridArrray[hexArrIndexes[0, 0], hexArrIndexes[0, 1]] = tempHex;


            /** -------  Swap the Names of Hexagons -------  **/

            string tempName = "Hex" + hexArrIndexes[2, 0] + hexArrIndexes[2, 1];
            gridArrray[hexArrIndexes[2, 0], hexArrIndexes[2, 1]].name = "Hex" + hexArrIndexes[2, 0] + hexArrIndexes[2, 1];
            gridArrray[hexArrIndexes[1, 0], hexArrIndexes[1, 1]].name = "Hex" + hexArrIndexes[1, 0] + hexArrIndexes[1, 1];
            gridArrray[hexArrIndexes[0, 0], hexArrIndexes[0, 1]].name = "Hex" + hexArrIndexes[0, 0] + hexArrIndexes[0, 1];

        }
    }

    public bool CheckForMatches()
    {
        List<GameObject> matches = new List<GameObject>();
        List<GameObject> tempMatches = new List<GameObject>();
        // For all hexagons in grid array
        for (int i = 0; i <= gridArrray.GetUpperBound(0) - 1; i++)
            for (int j = 0; j <= gridArrray.GetUpperBound(1); j++)
            {   // If i,j = i+1,j  continue. If not, there will not be any match
                if (gridArrray[i, j].GetComponent<Hex>().color == gridArrray[i + 1, j].GetComponent<Hex>().color)
                {
                    tempMatches.Add(gridArrray[i, j]);
                    tempMatches.Add(gridArrray[i + 1, j]);

                    int sign = i % 2 == 0 ? -1 : 1;      // Because, the even rows are located at lower heights than odd ones

                    // Desired hexagon should be at the (odd) top or the (even) bottom of the hexagon at right.
                    if (j + sign >= 0 && j + sign <= gridArrray.GetUpperBound(1))
                    {
                        if (gridArrray[i, j].GetComponent<Hex>().color == gridArrray[i + 1, j + sign].GetComponent<Hex>().color)
                        {
                            tempMatches.Add(gridArrray[i + 1, j + sign]);
                        }
                    }

                    // Desired hexagon should be at the (odd) bottom or the (even) top of the hexagon at left.
                    if (j - sign >= 0 && j - sign <= gridArrray.GetUpperBound(1))
                    {
                        if ((gridArrray[i, j].GetComponent<Hex>().color == gridArrray[i, j - sign].GetComponent<Hex>().color))
                        {
                            tempMatches.Add(gridArrray[i, j - sign]);
                        }
                    }

                    // If there are 3 or more matches, add them to the list
                    if (tempMatches.Count > 2)
                    {
                        foreach (GameObject obj in tempMatches)
                        {
                            if (!matches.Contains(obj))
                            {
                                matches.Add(obj);
                            }
                        }
                    }

                    tempMatches = new List<GameObject>();
                }
            }
        // If no match, return false
        if (matches.Count == 0)
            return false;
        // Else, blow all hexagons matched
        else
        {
            foreach (GameObject obj in matches)
            {
                obj.GetComponent<Hex>().Blow();
            }
            // And call the function
            StartCoroutine("WaitAndSlide");
            return true;
        }
    }
    // Make hexagons fall and instantiate new ones
    public void SlideHexesDown()
    {
        /** --------- This section will make all the hanging hexagons fall, both graphically and in the array --------- **/
        /** --------- As the result, only the top will be empty --------- **/
        falling = true;
        // For all indexes in array
        for (int i = 0; i < gridWidth; i++)
            for (int j = 0; j < gridHeight; j++)
            {
                // If there is a null
                if (gridArrray[i, j] == null)
                {
                    // Count the gap
                    for (int jNext = j; jNext < gridHeight; jNext++)
                    {
                        if (gridArrray[i, jNext] != null)
                        {   // Make the first non-null occurence to go down to the null's index
                            gridArrray[i, j] = gridArrray[i, jNext];
                            gridArrray[i, jNext] = null;

                            // And move it to the null's place visually.
                            GameObject temp = GameObject.Find("Hex" + i.ToString() + jNext.ToString());
                            temp.transform.Translate(new Vector3(0, -1 * hexHeight * (jNext - j), 1));
                            temp.name = "Hex" + i.ToString() + j.ToString();

                            break;
                        }
                    }
                }
            }

        /** --------- This section instantiates new hexagons and fills the top  --------- **/

        for (int i = 0; i < gridWidth; i++)
            for (int j = 0; j < gridHeight; j++)
            {
                if (gridArrray[i, j] == null)
                {
                    GameObject hex;
                    if (bombIsReady && Bomb.instance == null)
                    {
                        hex = Instantiate(bombPrefab
                                                                , new Vector2(i * hexWidth * 0.75f - rightPadding, j * hexHeight - topPadding - (i % 2 == 0 ? hexHeight / 2 : 0))
                                                                , Quaternion.identity);
                        bombIsReady = false;

                        hex.name = "Hex" + i + j;
                        gridArrray[i, j] = hex;

                    }
                    else
                    {
                        hex = Instantiate(hexagonPrefab
                                         , new Vector2(i * hexWidth * 0.75f - rightPadding, j * hexHeight - topPadding - (i % 2 == 0 ? hexHeight / 2 : 0))
                                         , Quaternion.identity);

                        hex.name = "Hex" + i + j;
                        gridArrray[i, j] = hex;

                    }

                    StartCoroutine("WaitAndCheck");

                }
            }
        falling = false;
    }

    IEnumerator WaitAndCheck()
    {
        yield return new WaitForSeconds(.5f);
        CheckForMatches();
    }
    IEnumerator WaitAndSlide()
    {
       while(falling == true) { 
        yield return new WaitForSeconds(.5f);
       }
        SlideHexesDown();
    }

    public void CheckIfGameOver()
    {
        // The game is not over if:
        for (int i = 0; i <= gridArrray.GetUpperBound(0) - 1; i++)
            for (int j = 0; j <= gridArrray.GetUpperBound(1); j++)
            {
                int ijColor = gridArrray[i, j].GetComponent<Hex>().color;

                int sign = i % 2 == 0 ? -1 : 1;

                // If 3 hexagons lined up horizontally
                    if (ijColor == gridArrray[i + 2, j].GetComponent<Hex>().color &&
                       (ijColor == gridArrray[i + 1, j].GetComponent<Hex>().color ||
                     // (even) look at the lower or (odd) look at the higher hexagon
                        ijColor == gridArrray[i + 1, j + sign].GetComponent<Hex>().color))
                    {
                        return;
                    }

                // If 2 horizontally adjacent hexagons exist as:

                // 1) 2 hexagons' have different j
                    if (ijColor == gridArrray[i + 1, j + sign].GetComponent<Hex>().color)
                    {
                        // Check upper 2nd hexagon at the lower hexagon and lower 2nd hexagon at the upper hexagon,
                            if (ijColor == gridArrray[i + 1, j - sign].GetComponent<Hex>().color &&
                        ijColor == gridArrray[i, j - 2].GetComponent<Hex>().color) { return; }
                    }

                // 2) 2 hexagons' have different j
                if (ijColor == gridArrray[i + 1, j].GetComponent<Hex>().color)
                {
                    // Check upper 2nd hexagon of the lower hexagon and lower 2nd hexagon of the upper hexagon,
                        if (ijColor == gridArrray[i + 1, j + 2 * sign].GetComponent<Hex>().color &&
                        ijColor == gridArrray[i, j - 2 * sign].GetComponent<Hex>().color) { return; }
                }


                // If 2 hexagons stacked, look at the corners of them
                sign = i % 2 == 0 ? -1 : 0;
                    if (ijColor == gridArrray[i, j + 1].GetComponent<Hex>().color &&              // If stacked,
                   (ijColor == gridArrray[i - 1, j + 2 + sign].GetComponent<Hex>().color ||   // top corners
                    ijColor == gridArrray[i + 1, j + 2 + sign].GetComponent<Hex>().color ||   // (odd) j+2 (even) j+1
                    ijColor == gridArrray[i + 1, j + sign].GetComponent<Hex>().color ||       // bottom corners
                    ijColor == gridArrray[i - 1, j + sign].GetComponent<Hex>().color))        // (odd) j  (even) j-1
                    {
                        return;
                    }
            }
        // Else, game is over.
        GameManager.instance.GameOver();
    }
}