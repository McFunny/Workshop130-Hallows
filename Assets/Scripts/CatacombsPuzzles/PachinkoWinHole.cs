using UnityEngine;

public class PachinkoWinHole : MonoBehaviour
{
    public int rewardAmount;

    public void Payout()
    {
        PachinkoManager.Instance.totalWinnings += rewardAmount;
        PachinkoManager.Instance.NotifyBugDestroyed(true, rewardAmount);

        if(this.gameObject.name == "Jackpot")
        {
            AchievementManager.Instance.NotifyPachinkoJackpot();
        }
    }

}
