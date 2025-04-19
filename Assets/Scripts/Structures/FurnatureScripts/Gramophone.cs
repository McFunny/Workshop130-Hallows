using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gramophone : FurnitureBehaviorScript
{
    public List<DiscAndSong> discPairs;

    public AudioSource source;

    //Still neds particles and anims

    public ParticleSystem musicParticles;

    void Awake()
    {
        savedItems.Add(null);
    }

    void Start()
    {
        base.Start();
        FurnitureStart();
    }

    public override void StructureInteraction()
    {
        if(absentFromGrid && !onTable) return; //makes the player actually have to take time to steal the furniture
        bool addedSuccessfully = false;
        //grabs the disc, otherwise picks it up
        if(savedItems.Count != 0 && savedItems[0] != null)
        {
            addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[0], 1);
            if (addedSuccessfully)
            {
                savedItems[0] = null;
                PlayerInventoryHolder.Instance.UpdateInventory();
                TurnOff();
            } 
            return;
        }

        addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item && (savedItems.Count == 0 || savedItems[0] == null))
        {
            InsertItem(item);
            return;
        }

        if(savedItems.Count != 0 && savedItems[0] != null)
        {
            bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[0], 1);
            if (addedSuccessfully)
            {
                savedItems[0] = null;
                PlayerInventoryHolder.Instance.UpdateInventory();
                TurnOff();
            } 
            return;
        }
    }

    void InsertItem(InventoryItemData _item)
    {
        for(int i = 0; i < discPairs.Count; i++)
        {
            if(discPairs[i].disc == _item)
            {
                savedItems[0] = _item;
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();

                source.clip = discPairs[i].song;
                AmbientAudioManager.Instance.StartGramophone(this, discPairs[i].song);
                musicParticles.Play();
                source.Play();
                return;
            }
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);

        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        OnFurnitureDestroy();
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        TurnOff();
    }

    void TurnOff()
    {
        source.Stop();
        musicParticles.Stop();
        if(AmbientAudioManager.Instance.playingGramophone != this.transform) return;
        AmbientAudioManager.Instance.EndGramophone();
    }
}
[System.Serializable]
public class DiscAndSong
{
    public InventoryItemData disc;
    public AudioClip song;
}
