using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPuzzleManager : MonoBehaviour
{
    public WaterPuzzleTile puzzle1;

    public WaterPuzzleTile puzzle2;

    public WaterPuzzleTile puzzle3;

    // Update is called once per frame
    void Update()
    {
        if (puzzle1.isSolved && puzzle2.isSolved && puzzle3.isSolved)
        {
            puzzle1.isLocked = true;
            puzzle2.isLocked = true;
            puzzle3.isLocked = true;
        }
    }

}
