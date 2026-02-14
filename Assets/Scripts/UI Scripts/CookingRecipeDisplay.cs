using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingRecipeDisplay : MonoBehaviour
{
    public List<RecipeImages> recipeSlots = new List<RecipeImages>();
}

[Serializable]
public class RecipeImages
{
    public Image images;
    public TextMeshProUGUI texts;
}
