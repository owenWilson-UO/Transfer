using System.Collections;
using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerUpgradeData playerUpgradeData;
    [SerializeField] GameObject magicCircle;
    [SerializeField] TransferThrowable tt;
    [SerializeField] ParticleSystem lightning;

    [Header("Settings")]
    [SerializeField] private float speed;

    public bool firstTimeGrabbed { get; private set; }

    private void Start()
    {
        firstTimeGrabbed = playerUpgradeData.maxTransferAmount == 0;

        if (!firstTimeGrabbed)
        {
            magicCircle.SetActive(false);
            lightning.Stop();
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
        lightning.Play();
        Vector3 _knifeRestScale = transform.localScale;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / spawnDuration);
            transform.localScale = new Vector3(
                _knifeRestScale.x,
                Mathf.Lerp(_knifeRestScale.y, 0f, p),
                _knifeRestScale.z
            );
            yield return null;
        }
        transform.localScale = Vector3.zero;
        lightning.Stop();
        gameObject.SetActive(false);

        if (playerUpgradeData.maxTransferAmount == 0)
        {
            tt.transferAmount = 1;
            playerUpgradeData.maxTransferAmount = 1;
            tt.SpawnHandKnife();
        }
    }
}
