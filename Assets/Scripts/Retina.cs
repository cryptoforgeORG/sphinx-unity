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

    public void Project(long chatId, long contactId, List<Msg> messages)
    {
        this.canvas_Feed.contactId = contactId;
        this.canvas_Feed.chatId = chatId;

        foreach (Msg message in messages)
        {
            this.canvas_Feed.AddItem(message);
        }
    }
}
