using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerisItem
{
    public string imagePath;
    public string prefabPath;
    public float Chance;
    public TerisItem(string imagepath,string prefabpath,float chance)
    {
        this.imagePath = imagepath;
        this.prefabPath = prefabpath;
        this.Chance = chance;
    }
   
}
