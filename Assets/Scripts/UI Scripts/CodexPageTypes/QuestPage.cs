using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestPage : CodexPage
{
    private QuestManager questManager;
    private Codex3 codex;
    private Quest currentOpenQuest;
    private ControlManager controlManager;
    [SerializeField] private VerticalLayoutGroup mainVert;
    [SerializeField] private GameObject rewardsContainer;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TextMeshProUGUI rewardName;
    [SerializeField] private ConfirmationBox confirmationBox;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI timeRemainingText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button removeQuestButton;

    [TextArea(4, 2)]
    [SerializeField] string deleteQuestText;

    [SerializeField] private Sprite mintSprite;
    [SerializeField] private List<Sprite> photoSprites;
    private void Awake()
    {
        questManager = FindFirstObjectByType<QuestManager>();
        codex = GetComponentInParent<Codex3>();
        //print(confirmationBox);
        controlManager = FindFirstObjectByType<ControlManager>();
    }

    private void OnEnable()
    {
        Canvas.ForceUpdateCanvases();
        controlManager.deleteQuest.action.started += ControllerDeleteQuest;
    }

    private void OnDisable()
    {
        currentOpenQuest = null;
        controlManager.deleteQuest.action.started -= ControllerDeleteQuest;
    }



    public override void UpdatePage(CodexEntries entry, Quest quest)
    {
        string assignee = quest.assignee.ToString();
        Debug.Log("Assignee: " + assignee);

        if(assignee == "ElderMandrake") assignee = "Elder Mandrake";
        else if(assignee == "MistMerchant") assignee = "Mist Merchant";

        Debug.Log("Formatted Assignee: " + assignee);

        title.text = assignee + ": " + quest.name;
        currentOpenQuest = quest;
        description.text = quest.description;

        if (quest.daysLeft >= 0)
        {
            if (quest.daysLeft == 1)
            {
                timeRemainingText.text = "Final Day to Complete";
                timeRemainingText.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                timeRemainingText.text = quest.daysLeft + " Days Left to Complete";
                timeRemainingText.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            timeRemainingText.transform.parent.gameObject.SetActive(false);
        }

        if (quest.displayProgress == false)
        {
            progressText.text = "";
            progressSlider.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            progressSlider.transform.parent.gameObject.SetActive(true);
            progressSlider.minValue = 0;
            progressSlider.maxValue = quest.maxProgress;
            progressSlider.value = quest.progress;
        }

        //Override for repeatable quests and stuff like that I dont know man I dont know anything ever
        UpdateQuestName(quest);
        UpdateQuestDescription(quest);

        if (quest.itemRewards.Count > 0)
        {
            rewardIcon.sprite = quest.itemRewards[0].icon;
            rewardName.text = "x" + quest.itemRewards.Count;
            rewardsContainer.SetActive(true);
        }
        else if (quest.mintReward > 0)
        {
            rewardIcon.sprite = mintSprite;
            rewardName.text = "x" + quest.mintReward;
            rewardsContainer.SetActive(true);
        }
        else
        {
            rewardsContainer.SetActive(false);
        }

        if(quest.isMajorQuest || quest.alreadyCompleted) removeQuestButton.gameObject.SetActive(false);
        else removeQuestButton.gameObject.SetActive(true);

        mainVert.enabled = false;
        mainVert.enabled = true; //Yeah of course the solution is to turn it off and then turn it back on

        image.sprite = photoSprites[(int)quest.assignee];
    }
    
    private void ControllerDeleteQuest(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(confirmationBox.gameObject.activeSelf) return;
        if (codex.menuIndex == 2) DeleteQuest();
    }

    public void DeleteQuest()
    {
        if (currentOpenQuest.isMajorQuest) return;
        OpenConfirmationBox(deleteQuestText);
    }

    public void OpenConfirmationBox(string message)
    {
        confirmationBox.messageText.text = message;
        if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
        confirmationBox.gameObject.SetActive(true);
        confirmationBox.yesButton.onClick.AddListener(YesPressed);
        confirmationBox.noButton.onClick.AddListener(NoPressed);
    }

    private void YesPressed()
    {
        confirmationBox.gameObject.SetActive(false);
        questManager.ForceRemoveQuest(currentOpenQuest);
        codex.Back();
    }

    private void NoPressed()
    {
        confirmationBox.gameObject.SetActive(false);
        if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(null);
    }

    private void UpdateQuestName(Quest quest)
    {
        var type = quest.GetType();
        string assignee = quest.assignee.ToString();

        if(assignee == "ElderMandrake") assignee = "Elder Mandrake";
        else if(assignee == "MistMerchant") assignee = "Mist Merchant";

        if (type.Equals(typeof(FetchQuest)))
        {
            //print("Fetch Quest");
            var q = quest as FetchQuest;
            var t = q.name;

            if (q.amount > 1 && !q.desiredItem.displayName.EndsWith("s")) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
            else t = t.Replace("{itemName}", q.desiredItem.displayName.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            title.text = assignee + ": " + t;
        }
        else if (type.Equals(typeof(HuntQuest)))
        {
            //print("Hunt Quest");
            var q = quest as HuntQuest;
            var t = q.name;

            if (q.amount > 1 && !q.targetCreature.name.EndsWith("s")) t = t.Replace("{creatureName}", q.targetCreature.name.ToString() + "s");
            else t = t.Replace("{creatureName}", q.targetCreature.name.ToString());

            t = t.Replace("{creatureAmount}", q.amount.ToString());

            title.text = assignee + ": " + t;
        }
        else if (type.Equals(typeof(GrowQuest)))
        {
            //print("Grow Quest");
            var q = quest as GrowQuest;
            var t = q.name;

            if (q.amount > 1 && !q.desiredItem.displayName.EndsWith("s")) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
            else t = t.Replace("{itemName}", q.desiredItem.displayName.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            title.text = assignee + ": " + t;
        }
        else
        {
            title.text = assignee + ": " + quest.name;
        }

        title.text = title.text.Replace("Null: ", "");
    }

    private void UpdateQuestDescription(Quest quest)
    {
        var type = quest.GetType();

        if (type.Equals(typeof(FetchQuest)))
        {
            //print("Fetch Quest");
            var q = quest as FetchQuest;
            var t = q.description;

            if (q.amount > 1 && !q.desiredItem.displayName.EndsWith("s")) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
            else t = t.Replace("{itemName}", q.desiredItem.displayName.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            description.text = t;
            //questProgressText.text = q.progress + "/" + q.maxProgress;

            /*if (q.maxProgress == 1 || q.desiredItem.displayName.EndsWith("s")) progressText.text = q.desiredItem.displayName + " handed in: " + q.progress + "/" + q.maxProgress;
            else progressText.text = q.desiredItem.displayName + "s handed in: " + q.progress + "/" + q.maxProgress;*/
            progressSlider.transform.parent.gameObject.SetActive(false);
            
        }
        else if (type.Equals(typeof(HuntQuest)))
        {
            //print("Hunt Quest");
            var q = quest as HuntQuest;
            var t = q.description;

            if (q.maxProgress != 1 && !q.targetCreature.name.EndsWith("s")) t = t.Replace("{itemName}", q.targetCreature.name.ToString() + "s");
            else t = t.Replace("{itemName}", q.targetCreature.name.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            description.text = t;

            if (q.maxProgress == 1 || q.targetCreature.name.EndsWith("s")) progressText.text = q.targetCreature.name.ToString() + " eliminated: " + q.progress + "/" + q.maxProgress;
            else progressText.text = q.targetCreature.name.ToString() + "s eliminated: " + q.progress + "/" + q.maxProgress;
        }
        else if (type.Equals(typeof(GrowQuest)))
        {
            //print("Grow Quest");
            var q = quest as GrowQuest;
            var t = q.description;

            if (q.maxProgress != 1 && !q.desiredItem.displayName.EndsWith("s")) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
            else t = t.Replace("{itemName}", q.desiredItem.displayName.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            description.text = t;
            //questProgressText.text = q.progress + "/" + q.maxProgress;

            if (q.maxProgress == 1 || q.desiredItem.displayName.EndsWith("s")) progressText.text = q.desiredItem.displayName + " grown: " + q.progress + "/" + q.maxProgress;
            else progressText.text = q.desiredItem.displayName + "s grown: " + q.progress + "/" + q.maxProgress;
        }
        else
        {
            progressText.text = "Progress: " + quest.progress + "/" + quest.maxProgress;
        }
    }
}
