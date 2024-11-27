using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to events for success and failure
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        // Add bonus time for a successful recipe delivery
        KitchenGameManager.Instance.AddTimeOnDishSubmit(10f);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        // Deduct time for an incorrect recipe delivery
        KitchenGameManager.Instance.DeductTimeOnDishSubmit(12f); // Example: deduct 12 seconds
    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                // Only accepts Plates
                DeliveryManager.Instance.DeliverRecipe(plateKitchenObject);

                // Destroy the kitchen object regardless of success or failure
                player.GetKitchenObject().DestroySelf();
            }
        }
    }
}