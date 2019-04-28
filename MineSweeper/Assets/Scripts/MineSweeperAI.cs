using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineSweeperAI : MonoBehaviour
{
    private GameMaster gm;
    float[,] possibilitys;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameMaster.GetInstance();
        possibilitys = new float[gm.X_SIZE, gm.Y_SIZE];
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
