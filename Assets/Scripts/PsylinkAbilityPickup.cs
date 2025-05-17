using System.Collections;
using UnityEngine;

public class PsylinkAbilityPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerUpgradeData playerUpgradeData;
    [SerializeField] GameObject magicCircle;
    [SerializeField] PsylinkThrowable pt;


    [Header("Settings")]
    [SerializeField] private float speed;

    public bool firstTimeGrabbed { get; private set; }

    private void Start()
    {
        firstTimeGrabbed = playerUpgradeData.maxPsylinkAmount == 0;

        if (!firstTimeGrabbed)
        {
            magicCircle.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Despawn());
        }

        magicCircle.SetActive(false);
    }

    private IEnumerator Despawn()
    {
        float elapsed = 0f;
        float spawnDuration = 0.2f;
        Vector3 _knifeRestScale = transform.localScale;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / spawnDuration);
            transform.localScale = new Vector3(
                Mathf.Lerp(_knifeRestScale.x, 0f, p),
                Mathf.Lerp(_knifeRestScale.y, 0f, p),
                Mathf.Lerp(_knifeRestScale.z, 0f, p)
            );
            yield return null;
        }
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);

        if (playerUpgradeData.maxPsylinkAmount == 0)
        {
            playerUpgradeData.maxPsylinkAmount = 1;
        }
    }
}
