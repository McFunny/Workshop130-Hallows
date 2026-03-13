using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CookingRecipeBook : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private RecipeButtonID buttonPrefab;
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] private Transform leftPage, rightPage;
    [SerializeField] private TextMeshProUGUI nameText, timesMadeText, restoresText;
    [SerializeField] private UILerp uiLerp;
    [SerializeField] private GameObject controllerPrompts, kbmPrompts;
    [SerializeField] private AudioClip openSound, closeSound, pageTurnSound;
    [SerializeField] private float openVolume, closeVolume, pageTurnVolume;
    [SerializeField] private List<CookingRecipeDisplay> recipeDisplays = new List<CookingRecipeDisplay>();
    private List<GameObject> pages = new List<GameObject>();
    private List<RecipeButtonID> recipeButtons = new List<RecipeButtonID>();
    public static bool recipeBookOpen = false;
    private const int PAGE_ENTRY_CAP = 20;
    private int currentPage = 0;
    private int totalPages = 0;

    // Start is called before the first frame update
    void Start()
    {
        container.SetActive(false);
        rightPage.gameObject.SetActive(false);
        PopulateBook();
    }

    // Update is called once per frame
    void Update()
    {
        /*//DEBUG ----------
        if(Input.GetKeyDown(KeyCode.RightAlt))
        {
            if(recipeBookOpen == true)
            {
                CloseRecipeBook();
            }
            else OpenRecipeBook();
        }
        //DEBUG ----------*/

        if(recipeBookOpen == false) return;

        if(Input.GetKeyDown(KeyCode.D))
        {
            ChangePage(1);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ChangePage(-1);
        }

        if(ControlManager.isController)
        {
            controllerPrompts.SetActive(true);
            kbmPrompts.SetActive(false);

            if(Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                ChangePage(1);
            }
            else if (Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                ChangePage(-1);
            }

            
            if(Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                CloseRecipeBook();
            }

            if(EventSystem.current.currentSelectedGameObject == null) EventSystem.current.SetSelectedGameObject(pages[currentPage].transform.GetChild(0).gameObject);

        }
        else
        {
            controllerPrompts.SetActive(false);
            kbmPrompts.SetActive(true);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CloseRecipeBook();
        }
    }

    public void OpenRecipeBook()
    {
        //container.SetActive(!container.activeSelf);
        EventSystem.current.SetSelectedGameObject(null);
        
        container.SetActive(true);
        Time.timeScale = 0f;
        PlayerMovement.restrictMovementTokens++;
        UpdateEntries();
        recipeBookOpen = true;
        uiLerp.lerpToStart = true;
        AudioPoolManager.Instance.PlayClip(openSound, openVolume);
        
    }

    public void CloseRecipeBook()
    {
        Time.timeScale = 1f;
        PlayerMovement.restrictMovementTokens--;
        rightPage.gameObject.SetActive(false);
        recipeBookOpen = false;
        uiLerp.lerpToStart = false;
        AudioPoolManager.Instance.PlayClip(closeSound, closeVolume);
    }

    private void PopulateBook()
    {
        foreach(Transform child in leftPage)
        {
            Destroy(child.gameObject);
        }


        var cookingDatabase = CookingDatabase.Instance.GetCraftingDatabase();
        int entriesOnCurrentPage = 0;
        GameObject currentPage = Instantiate(pagePrefab, leftPage);
        pages.Add(currentPage);
        currentPage.name = "Page";
        totalPages ++;

        Debug.Log(cookingDatabase.Count);
        for (int i = 0; i < cookingDatabase.Count; i++)
        {

            //Creates new page when entry cap is reached
            if(currentPage.transform.childCount == PAGE_ENTRY_CAP)
            {
                currentPage = Instantiate(pagePrefab, leftPage);
                currentPage.name = "Page";
                entriesOnCurrentPage = 0;
                pages.Add(currentPage);
                totalPages ++;
                currentPage.SetActive(false);
            }

            RecipeButtonID button = Instantiate(buttonPrefab, currentPage.transform);
            button.image.sprite = cookingDatabase[i].output.icon;
            button.cookingRecipeBook = this;
            button.assignedRecipe = cookingDatabase[i];
            recipeButtons.Add(button);
            entriesOnCurrentPage++;
        }
    }

    public void UpdateEntries()
    {
        var cookingDatabase = CookingDatabase.Instance.GetCraftingDatabase();
        bool allUnlocked = true;
        for (int i = 0; i < cookingDatabase.Count; i++)
        {
            if(cookingDatabase[i].amountMade > 0 || cookingDatabase[i].unlocked == true)
            {
                if(cookingDatabase[i].amountMade == 0) allUnlocked = false;
                cookingDatabase[i].unlocked = true;
                LockOrUnlockRecipe(recipeButtons[i], true);
            }
            else
            {
                LockOrUnlockRecipe(recipeButtons[i], false);
                allUnlocked = false;
            }
        }

        if(allUnlocked) AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Competent_Cook);
    }

    private void LockOrUnlockRecipe(RecipeButtonID button, bool unlocked)
    {
        if(unlocked == true)
        {
            button.image.gameObject.SetActive(true);
            button.questionMark.gameObject.SetActive(false);
            button.recipeName.text = button.assignedRecipe.output.displayName;
        }
        else
        {
            button.image.gameObject.SetActive(false);
            button.questionMark.gameObject.SetActive(true);
            button.recipeName.text = "???";
        }
    }

    public void ShowEntry(CookingRecipe recipe)
    {
        rightPage.gameObject.SetActive(true);

        for(int i = 0; i < recipeDisplays.Count; i++)
        {
            if(recipe.validRecipes.Count > i)
            {
                ApplyValidRecipe(recipe.validRecipes[i], recipeDisplays[i]);
            }
            else
            {
                ApplyNoRecipe(recipeDisplays[i]);
            }
        }

        nameText.text = recipe.output.displayName;
        timesMadeText.text = "Times Made: " + recipe.amountMade;
        restoresText.text = "Restores " + recipe.output.staminaValue + " Stamina";
    }

    private void ApplyValidRecipe(ValidRecipe validRecipe, CookingRecipeDisplay display)
    {
        for (int i = 0; i < display.recipeSlots.Count; i++)
        {
            display.recipeSlots[i].images.sprite = validRecipe.usedItems[i].icon;
            display.recipeSlots[i].images.enabled = true;
            display.recipeSlots[i].texts.gameObject.SetActive(false); //Question Mark for missing recipe
        }
    }

    private void ApplyNoRecipe(CookingRecipeDisplay display)
    {
        for (int i = 0; i < display.recipeSlots.Count; i++)
        {
            display.recipeSlots[i].images.enabled = false;
            display.recipeSlots[i].texts.gameObject.SetActive(true); //Question Mark for missing recipe
        }
    }

    private void ChangePage(int val)
    {
        if (!AreThereEnoughPages(val)) return;

        foreach(UIMenuButton button in pages[currentPage].GetComponentsInChildren<UIMenuButton>())
        {
            button.isSelected = false;
            button.arrowImage.color = button.c_invisible;
        }

        currentPage += val;
        

        foreach(GameObject page in pages)
        {
            page.SetActive(false);
        }
        
        pages[currentPage].SetActive(true);

        EventSystem.current.SetSelectedGameObject(pages[currentPage].transform.GetChild(0).gameObject);
        AudioPoolManager.Instance.PlayClip(pageTurnSound, pageTurnVolume);
    }

    private bool AreThereEnoughPages(int incrementDirection)
    {
        // Check if there are enough pages to display
        if (incrementDirection == 1)
        {
            if (currentPage == totalPages - 1) return false;
            else return true;
        }
        else if (incrementDirection == -1)
        {
            if (currentPage == 0) return false;
            else return true;
        }
        Debug.LogError("Invalid increment direction: " + incrementDirection);
        return false; // If we get here there is a problem please help
    }
}