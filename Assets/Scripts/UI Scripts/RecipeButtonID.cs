using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeButtonID : MonoBehaviour
{
    public CookingRecipeBook cookingRecipeBook;
    public CookingRecipe assignedRecipe;
    public Image image;
    public TextMeshProUGUI recipeName;
    public TextMeshProUGUI questionMark;

    public void OnClick()
    {
        if(assignedRecipe.amountMade <= 0) return;
        Debug.Log("Recipe is Unlocked");
        cookingRecipeBook.ShowEntry(assignedRecipe);
    }

}
