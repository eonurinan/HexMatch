using UnityEngine;

public class InputManager : MonoBehaviour
{
    private GameObject searcher;    // Looks for nearest 3 hexes
    public static bool searching;   // Indicates if searching or not
    private Vector3 posStart, posEnd;
    private float dragThreshold;

    void Start()
    {
        dragThreshold = Screen.width * .2f;
        searcher = Resources.Load<GameObject>("Prefabs/Circle");
    }

    void Update()
    {
        if (Input.touchCount > 0)
            if (GameManager.instance.rotating == false)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    posStart = touch.position;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    posEnd = touch.position;

                    // Check if it's a swipe
                    if (Mathf.Abs(posStart.x - posEnd.x) > dragThreshold && GridManager.instance.selected !=null)
                    {
                        if (posStart.x > posEnd.x)
                        {   //Right swipe
                            GameManager.instance.RotateCheck3(true);
                        }
                        else
                        {   //Left swipe
                            GameManager.instance.RotateCheck3(false);
                        }
                    }

                    // Or a tap
                    else
                    {
                        Vector3 pos = Camera.main.ScreenToWorldPoint(posEnd);
                        Selector.instance.Deselect();
                        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

                        if (!(hit == null) && hit.collider != null)
                        {
                            GameObject tempSearcher = Instantiate(searcher, pos, Quaternion.identity);
                        }
                    }
                }
            }
    }
}

