using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    private ControlManager controlManager;
    public Button button;

    private void OnEnable()
    {
        if (controlManager == null) controlManager = FindFirstObjectByType<ControlManager>();
        controlManager.codexSelect.action.started += InvokeButtonPress;
        controlManager.backCodex.action.started += CancelRename;
    }

    private void OnDisable()
    {
        controlManager.codexSelect.action.started -= InvokeButtonPress;
        controlManager.backCodex.action.started -= CancelRename;
    }

    public void UpdateCritterName(bool wasCancelled = false)
    {
        var newName = critterName.text;
        print("New Name Length = " + newName.Length);
        if (newName.Length == 0 || wasCancelled)
        {
            Debug.LogWarning("Critter or Pet name cannot be empty.");

            if (assignedCritter != null)
            {
                critterName.text = assignedCritter.name;
                print("Resetting to critter name: " + assignedCritter.name);
            }

            else if (assignedPet != null)
            {
                critterName.text = assignedPet.name;
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

    private void InvokeButtonPress(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == button.gameObject)
        {
            print("Controller Rename Started");
            button.onClick.Invoke();
        }
    }

    private void CancelRename(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == critterName.gameObject)
        {
            UpdateCritterName(true);
            EventSystem.current.SetSelectedGameObject(button.gameObject); // fix this
        }
    }
}
