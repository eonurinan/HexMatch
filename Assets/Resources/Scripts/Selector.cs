using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    public static Selector instance = null;     // Singleton instance

    // Singleton initialization
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
    }

    // Sets the parentOfSelected as parent of the selected 3 objects
    private void GroupHexes()
    {
        GameObject parent = GridManager.instance.parentOfSelected;
        Vector3 pos = new Vector3();

        // Finding the midpoint to set position of parentOfSelected
        foreach (GameObject obj in GridManager.instance.selected)
        {
            pos += obj.transform.position;
        }

        parent.transform.position = pos / 3;

        // Sets parentOfSelected as selected objects' parent
        foreach (GameObject obj in GridManager.instance.selected)
        {
            obj.transform.SetParent(parent.transform);
        }
    }

    // Turns the outlines on of all the selected objects
    public void Highlight(List<GameObject> toHighlight)
    {
        foreach (GameObject obj in toHighlight)
        {
            obj.GetComponent<Hex>().Highlight(true);
        }
    }

    // Turns the outlines off of all the objects in the grid
    public void DeighlightAll()
    {
        foreach (GameObject obj in GridManager.instance.gridArrray)
        {
            obj.GetComponent<Hex>().Highlight(false);
        }
    }

    // Groups and highlights objects
    public void Select(List<GameObject> toSelect)
    {
        GridManager.instance.selected = toSelect;
        GroupHexes();
        Highlight(toSelect);
    }
    // Ungroups and dehighlights objects
    public void Deselect()
    {
        if (GridManager.instance.selected != null)
        {
            foreach (GameObject obj in GridManager.instance.selected)
            {
                if (!(obj == null))
                {
                    obj.transform.SetParent(null);
                    obj.GetComponent<Hex>().Highlight(false);
                }
            }
            GridManager.instance.selected = null;
        }
    }
}