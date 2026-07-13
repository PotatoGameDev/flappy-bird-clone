using UnityEngine;

public class EnergySuckerController : MonoBehaviour
{
    [SerializeField] private FlyingSaucerController flyingSaucerController;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (flyingSaucerController.state != FlyingSaucerState.ACTIVE)
        {
            return;
        }

        if (collider.CompareTag("Energy"))
        {
            EnergyBallController energy = collider.GetComponent<EnergyBallController>();

            // The UFOs repair with energy!
            if (energy.Type == EnergyType.CollectEnergy
                    && energy.energyValue > 0
                    && !flyingSaucerController.IsFullHealth())
            {
                energy.SetTargetTransform(flyingSaucerController.transform);
            }
        }
    }
}
