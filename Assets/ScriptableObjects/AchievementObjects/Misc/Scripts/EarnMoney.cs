using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarnMoney : AchievementObject
{

    public override void CheckProgress(float number = 1)
    {
        if (GameSaveData.Instance != null)
        {
            ResetProgress();
            AddProgress(GameSaveData.Instance.pTotalMoneyEarned);
        }
    }
}
