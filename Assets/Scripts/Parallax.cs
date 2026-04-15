using UnityEngine;
using System.Collections.Generic;

public class Parallax : MonoBehaviour
{
    [SerializeField] private GameObject parallaxPrefab;
    [SerializeField] private int preload = 3;

    // The component is nested in the main camera,
    // so the background has to move backwards
    [SerializeField] private float speed = -1;
    [SerializeField] private float verticalFactor = 0.1f;

    // How far in advance to mage the instances
    [SerializeField] private float minInitialOffsetX = 0f;
    [SerializeField] private float maxInitialOffsetX = 0f;

    // The background item can be distributed along the Y axis
    [SerializeField] private float deviationY = 0f;

    [SerializeField] private float vanishingOffsetX = 10f;

    //
    private List<ParallaxBackground> initialized;

    const int safeguard = 10;

    [SerializeField] private Transform cameraTarget;

    void Awake()
    {
        initialized = new List<ParallaxBackground>();

        Vector3 pos = transform.position;

        for (int i = 0; i < preload; i++)
        {
            GameObject background = SpawnBackground(pos);

            if (background == null)
            {
                return;
            }

            ParallaxBackground bg = background.GetComponent<ParallaxBackground>();
            float imageWidth = bg.imageWidth;

            pos.x += imageWidth;
        }
    }

    void Update()
    {
        PlanetController player = GameplayManager.Instance.Player;
        if (player.Dead) return;

        ParallaxBackground leader = initialized[0];

        if (IsPassed(leader))
        {
            ParallaxBackground last = initialized[^1];
            Vector3 lastPos = last.transform.position;

            initialized.RemoveAt(0);
            Destroy(leader.gameObject);

            SpawnBackground(lastPos + new Vector3(last.imageWidth, 0f, 0f));
        }

        foreach (ParallaxBackground bg in initialized)
        {
            Vector2 pos = bg.transform.position;
            pos.x += speed * Time.deltaTime;
            pos.y = verticalFactor * cameraTarget.transform.position.y;

            bg.transform.position = pos;
        }
    }

    GameObject SpawnBackground(Vector2 pos)
    {
        if (initialized.Count >= safeguard)
        {
            return null;
        }

        if (minInitialOffsetX > 0f)
        {
            // This means we are in offset mode, not one-after-another mode
            // In one-after-another we place next element after the last one.
            // In offset mode, we spawn it somewhere to the right.
            pos.x = transform.position.x + Random.Range(minInitialOffsetX, maxInitialOffsetX);
        }
        pos.y += Random.Range(-deviationY, deviationY);

        GameObject inst = Instantiate(parallaxPrefab, pos, Quaternion.identity, transform);
        initialized.Add(inst.GetComponent<ParallaxBackground>());

        return inst;
    }

    bool IsPassed(ParallaxBackground obj)
    {
        // TODO: Maybe this should be based on the actual sprite width?
        return obj.transform.position.x + vanishingOffsetX < transform.position.x;
    }
}
