//
//  http://playentertainment.company
//  
//  Copyright (c) Play Entertainment LLC, California. All rights reserved.
//

using UnityEngine;
using System.Collections;

public class Playsound : MonoBehaviour
{
    public void Clicky()
    {
        GetComponent<AudioSource>().Play();
    }
}
