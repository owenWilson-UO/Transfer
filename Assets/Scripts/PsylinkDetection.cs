using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class PsylinkDetection : MonoBehaviour
{
    public Rigidbody rb {  get; private set; }
    PsylinkThrowable pt;
    public bool targetHit {  get; private set; }

    private bool isSpinning;

    [Header("Still version Settings")]
    public float spinSpeed = 1f;
    public bool playSpawnAnimation = false;
    public bool playDestroyAnimation = false;
    public ParticleSystem spawnParticles;
    private Vector3 _knifeRestScale = new Vector3(0.2f, 0.2f, 0.2f);
    private Coroutine spawnCoroutine;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        pt = FindFirstObjectByType<PsylinkThrowable>();

        isSpinning = true;
        playSpawnAnimation = false;
        playDestroyAnimation = false;
    }

    private void Update()
    {
        if (playSpawnAnimation)
        {
            playSpawnAnimation = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(PrintCoroutine());
        }

        if (playDestroyAnimation)
        {
            playDestroyAnimation = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(DestroyCoroutine());
        }

        if (isSpinning)
        {
            transform.Rotate(0f, 1080f * spinSpeed * Time.deltaTime, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //logic for stopping the psylink when it hits a psylink interactable object
        if (!other.gameObject.CompareTag("PsylinkInteractable")) { return; }
        pt.readyToThrow = true;
        isSpinning = false;
        if (targetHit)
        {
            return;
        } 
        else
        {
            foreach (var item in pt.activePsylinks.ToList()) //use ToList to avoid modifying while iterating
            {
                if (item.obj == other.gameObject)
                {
                    Destroy(item.psylink); 
                    pt.activePsylinks.Remove(item);
                }
            }

            pt.activePsylinks.Add(new PsylinkAndObject()
            {
                obj = other.gameObject,
                psylink = gameObject
            });
            Debug.Log(pt.activePsylinks.ToString());
            targetHit = true;
        }
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.SetParent(other.transform);
    }

    private IEnumerator PrintCoroutine()
    {
        float elapsed = 0f;
        float spawnDuration = 0.2f;
        transform.localScale = Vector3.zero;
        spawnParticles.Play();

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / spawnDuration);
            transform.localScale = new Vector3(
                Mathf.Lerp(0f, _knifeRestScale.x, p),
                Mathf.Lerp(0f, _knifeRestScale.y, p),
                Mathf.Lerp(0f, _knifeRestScale.z, p)
            );
            yield return null;
        }

        // ensure exact final scale
        transform.localScale = _knifeRestScale;

        yield return new WaitForSeconds(spawnDuration);

        spawnParticles.Stop();
        spawnCoroutine = null;
    }

    private IEnumerator DestroyCoroutine()
    {
        float elapsed = 0f;
        float spawnDuration = 0.2f;
        spawnParticles.Play();

        yield return new WaitForSeconds(spawnDuration);

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

        // ensure exact final scale
        transform.localScale = Vector3.zero;

        spawnParticles.Stop();
        spawnCoroutine = null;
    }

    public void StopSpawnCoroutine()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnParticles.Stop();
    }
}
