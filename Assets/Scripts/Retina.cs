//
//  http://playentertainment.company
//  

using QFSW.QC;

using PlayEntertainment.Sphinx;

using System.Collections.Generic;
using UnityEngine;

[CommandPrefix("show")]
public class Retina : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject background;

    public Canvas_Feed canvas_Feed;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [Command(".retina")]
    public void Show()
    {
        this.background.SetActive(true);
        this.canvas_Feed.gameObject.SetActive(true);
    }

    public void AddToFeed(List<Msg> messages)
    {
        foreach (Msg message in messages)
        {
            this.canvas_Feed.AddItem(message);
        }
    }
}
