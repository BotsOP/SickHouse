using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using EventType = Managers.EventType;

/*
 * Tutorial:
   The tutorial is a separate scene that can be accessed through a button on the main menu.
   The scene has a smaller map and an infinitely slow wall.
   All of the UI elements are disabled except for one. 
   The player is expected to click on it and use it. When the game detects that it has been used properly, its function disables while the button stays on screen. 
   The next button is shown on the screen and enabled.
   
   The order and criteria are:
    - Raccoon purchasing button. After one is purchased, disable the ability
    - Tree brush. After placing 3, disable the ability
    - Water brush. After placing about 10, disable the ability
    - Eraser. After erasing 10 things, disable the ability
    - Beaver purchasing button. After one is purchased, disable the ability
    - When all of these buttons are available, accelerate the wall, enable everything, and reveal a button to head back to the main menu

 */
public class Tutorial : MonoBehaviour
{
    private enum TutorialOrder
    {
        RACCOON,
        TREE,
        WATER,
        DIRT,
        BEAVER,
        DONE,
    }
    
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Button raccoonButton;
    [SerializeField] private Button beaverButton;
    [SerializeField] private Button treeButton;
    [SerializeField] private Button waterButton;
    [SerializeField] private Button dirtButton;
    
    [SerializeField] private Color disabledColor = Color.gray;

    [SerializeField] private int amountTreesNeedToBePlaced = 10;
    [SerializeField] private int amountWaterNeedToBePlaced = 10;
    [SerializeField] private int amountDirtNeedToBePlaced = 10;
    [SerializeField] private float wallCycleInSeconds = 1;

    private int amountTreesPlaced;
    private int amountWaterPlaced;
    private int amountDirtPlaced;
    private Color enabledColor = Color.white;

    private void OnDisable()
    {
        EventSystem<EntityTileID>.Unsubscribe(EventType.CHANGED_TILE, PlacedTile);
    }
    private void Awake()
    {
        EventSystem<EntityTileID>.Subscribe(EventType.CHANGED_TILE, PlacedTile);
        
        DisableButton(beaverButton);
        DisableButton(treeButton);
        DisableButton(waterButton);
        DisableButton(dirtButton);
        
        EnableButton(raccoonButton);
    }

    private void PlacedTile(EntityTileID tileID)
    {
        switch (tileID)
        {
            case EntityTileID.TREE:
                amountTreesPlaced++;
                if (amountTreesPlaced >= amountTreesNeedToBePlaced)
                {
                    DisableButton(treeButton);
                    EnableButton(waterButton);
                    amountTreesNeedToBePlaced = int.MaxValue;
                }
                break;
            case EntityTileID.WATER:
                amountWaterPlaced++;
                if (amountWaterPlaced >= amountWaterNeedToBePlaced)
                {
                    DisableButton(waterButton);
                    EnableButton(dirtButton);
                    amountWaterNeedToBePlaced = int.MaxValue;
                }
                break;
            case EntityTileID.DIRT:
                amountDirtPlaced++;
                if (amountDirtPlaced >= amountDirtNeedToBePlaced)
                {
                    DisableButton(dirtButton);
                    EnableButton(beaverButton);
                    amountDirtNeedToBePlaced = int.MaxValue;
                }
                break;
        }
    }

    private void DisableButton(Button button)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = disabledColor;
        button.colors = colorBlock;
        button.interactable = false;
    }
    
    private void EnableButton(Button button)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = enabledColor;
        button.colors = colorBlock;
        button.interactable = true;
    }

    public void ClickedRaccoon()
    {
        DisableButton(raccoonButton);
        EnableButton(treeButton);
    }

    public void ClickedBeaver()
    {
        EnableButton(beaverButton);
        EnableButton(treeButton);
        EnableButton(waterButton);
        EnableButton(dirtButton);
        EnableButton(raccoonButton);

        gridManager.wallCycleInSeconds = wallCycleInSeconds;
    }
}
