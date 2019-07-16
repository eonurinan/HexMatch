using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Searcher : MonoBehaviour // Finds the nearest n objects to the clicked point.
{
    private List<GameObject> closestN;      //n closest hexagons
    private CircleCollider2D searcher;      //Collider of gameobject

    void Start()
    {
        searcher = GetComponent<CircleCollider2D>();
        closestN = new List<GameObject>();
        StartCoroutine("Search");
    }

    void Update()
    {
        if (closestN.Count >= 3 || searcher.radius > 100)                 //  The idea here is to increase the radius of the collider
                                                                          //  upon detecting 3 nearest hexagons.
        {
            Selector.instance.Select(closestN); //  When 3 hexagons found, pass them to the "Selector" class
            Destroy(gameObject);                //  And destroy the searcher.
        }
    }

    void OnTriggerEnter2D(Collider2D Other)
    {
        // If it's the first detected object, add it directly to the list.
        if (closestN.Count == 0)
        {
            closestN.Add(Other.gameObject);
        }
        //Else, compare the resulting patterns for each added object.
        else
        {
            float xPos = Other.transform.position.x;
            float yPos = Other.transform.position.y;

            int counterX = 0;
            int counterY = 0;

            foreach (GameObject obj in closestN)
            {
                counterX += obj.transform.position.x == xPos ? 1 : 0;            // How many hexagons exists in the list at same x
                counterY += obj.transform.position.y == yPos ? 1 : 0;            // or same y coordinates as the new hexagon's

            }
            //For example
            if (counterY == 0)                  // There should not be any same y coordinates for the desired pattern
            {
                if (counterX < 2)               // And no 3 stacked hexagons after new hexagon
                {

                    if (closestN.Count == 1)    // If it's 2nd hexagon so far, add it to the list 
                    {
                        closestN.Add(Other.gameObject);

                    }
                    else if (closestN.Count == 2)   // If it's 3rd,
                    {
                        float y1 = closestN[0].transform.position.y;
                        float y2 = closestN[1].transform.position.y;

                        float x1 = closestN[0].transform.position.x;
                        float x2 = closestN[1].transform.position.x;

                        if (xPos != x1 && xPos != x2)               //If previous 2 hexagons are stacked,
                        {
                            if ((yPos > y1 && yPos < y2) || (yPos > y2 && yPos < y1)) // 3rd should be in between the other 2 hexagons vertically
                            {
                                closestN.Add(Other.gameObject);
                            }
                        }


                        else if (yPos < (y1 > y2 ? y1 : y2) && xPos == (y1 > y2 ? x1 : x2)  // Else, it should be on the top of the hexagon at lower 
                              || yPos > (y1 < y2 ? y1 : y2) && xPos == (y1 < y2 ? x1 : x2)) // or under the hexagon at higher y coordinates.
                        {
                            closestN.Add(Other.gameObject);
                        }
                    }
                }
            }
        }

        closestN = closestN.OrderBy(obj => obj.name).ToList();      // Sort the list by names, to sort them by 
                                                                    // their indexes to have a solid rotation.
    }

    IEnumerator Search()
    {
        int multiplier = 0;
        while (closestN.Count < 3 && searcher.radius < 100)        // Expand the collider upon detecting 3 hexagons.
        {
                                                                   // Increasing the radius of the collider by 0.1 x multiplier
            searcher.radius += multiplier * .1f;                   // Because multiplier increases by 1 at each iteration,
            multiplier++;                                          // radius will increase 0.1 at 1st, 0.2 at 2nd 0.3 at 3rd iteration
            yield return null;                                     // and so on. This will provide both sensitive and fast search.
        }
    }
}