using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexCritter : MonoBehaviour
{
    public TMP_InputField critterName;
    public Image critterIcon;
    public Image homeIcon;
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public TextMeshProUGUI friendshipText;
    public CritterBehaviorScript assignedCritter;
    public PetBehaviorScript assignedPet;

    public void UpdateCritterName(TMP_InputField name)
    {
        var newName = name.text;
        print("New Name Length = " + newName.Length);
        if (newName.Length == 0)
        {
            Debug.LogWarning("Critter or Pet name cannot be empty.");
            
            if (assignedCritter != null)
            {
                name.text = assignedCritter.name;
                print("Resetting to critter name: " + assignedCritter.name);
            }

            else if (assignedPet != null)
            {
                name.text = assignedPet.name;
                print("Resetting to pet name: " + assignedPet.name);
            } 
            
            return;
        }

        if (assignedCritter != null)
        {
            assignedCritter.name = newName;
            print("Critter renamed to: " + assignedCritter.name);
        }
        else if (assignedPet != null)
        {
            assignedPet.name = newName;
            print("Pet renamed to: " + assignedPet.name);
        }
        else
        {
            Debug.LogWarning("No assigned critter or pet to update name.");
        }
        Codex3.isRenamingCritter = false;
    }

    public void OnSelect()
    {
        Codex3.isRenamingCritter = true;
    }

    public void OnDeselect()
    {
        Codex3.isRenamingCritter = false;
    }
}
