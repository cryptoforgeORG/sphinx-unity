//
//  http://playentertainment.company
//  

using PlayEntertainment.Sphinx;

using UnityEngine;

public class Canvas_Feed : MonoBehaviour
{
    public GameObject prefab_Cell;

    public Transform container;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddItem(Msg message)
    {
        GameObject instance = GameObject.Instantiate(this.prefab_Cell);

        instance.transform.SetParent(this.container, false);
        instance.GetComponent<Cell>().message = message;
        instance.GetComponent<Cell>().Activate();
    }
}
