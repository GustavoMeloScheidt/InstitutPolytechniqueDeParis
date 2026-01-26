using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Mole : MonoBehaviour {
    [Header("Graphics")]
    [Tooltip("Assign the specific MeshRenderer for the Standard Mole body here")]
    [SerializeField] private Renderer standardMoleRenderer;
    [Tooltip("Assign the specific MeshRenderer for the Hard Hat Mole body here")]
    [SerializeField] private Renderer hardHatMoleRenderer;
    [Tooltip("Assign the specific MeshRenderer for the Bomb here")]
    [SerializeField] private Renderer bombRenderer;
    [Tooltip("Assign the Border Renderer here")]
    [SerializeField] private Renderer borderRenderer;

    [Header("Visual References")]
    [SerializeField] private GameObject standardMoleObject;
    [SerializeField] private GameObject hardHatMoleObject;
    [SerializeField] private GameObject bombObject;
    [SerializeField] private GameObject hatVisualObject; 

    [Header("GameManager")]
    [SerializeField] private GameManager gameManager;

    [Header("Movement")]
    [SerializeField] private Transform moleMoveContainer; 
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private Vector3 endPosition = new Vector3(0f, -1.5f, 0f);

    [Header("Particles")]
    [SerializeField] private ParticleSystem dustParticles; 
    [SerializeField] private ParticleSystem hitParticles;  
    [SerializeField] private ParticleSystem bombParticles; 

    [Header("Audio")]
    [SerializeField] private AudioClip appearClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip hardHatClankClip; 
    [SerializeField] private AudioClip bombClip;

    [Header("Configuration")]
    [Tooltip("Time in seconds the mole stays visible after being hit before hiding")]
    [SerializeField] private float deathDelayDuration = 0.5f;

    // Internal variables
    private float showDuration = 0.5f;
    private float duration = 1f;
    
    private Color originalBorderColor;
    private Color stdColorStart;
    private Color hatColorStart;
    private Color bombColorStart;

    private AudioSource audioSource;
    private Coroutine movementCoroutine; 

    private bool hittable = true;
    public enum MoleType { Standard, HardHat, Bomb };
    private MoleType moleType;
    private float hardRate = 0.25f;
    private float bombRate = 0f;
    private int lives;
    private int moleIndex = 0;
    
    private Renderer activeRenderer;

    private void Awake() {
        audioSource = GetComponent<AudioSource>(); 
        
        if (borderRenderer != null) originalBorderColor = borderRenderer.material.color;

        // Save original colors to restore them later
        if (standardMoleRenderer != null) stdColorStart = standardMoleRenderer.material.color;
        if (hardHatMoleRenderer != null) hatColorStart = hardHatMoleRenderer.material.color;
        if (bombRenderer != null) bombColorStart = bombRenderer.material.color;

        if (moleMoveContainer == null) {
            Debug.LogError("MoleMoveContainer not assigned!");
            return;
        }

        moleMoveContainer.localPosition = startPosition;
        standardMoleObject.SetActive(false);
        hardHatMoleObject.SetActive(false);
        bombObject.SetActive(false);
    }

    public void Activate(int level) {
        SetLevel(level);
        CreateNext();
        
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveUpAndDown());
    }

    private void CreateNext() {
        // Reset border color
        if (borderRenderer != null) borderRenderer.material.color = originalBorderColor;

        float random = Random.Range(0f, 1f);

        standardMoleObject.SetActive(false);
        hardHatMoleObject.SetActive(false);
        bombObject.SetActive(false);

        if (random < bombRate) {
            moleType = MoleType.Bomb;
            bombObject.SetActive(true);
            activeRenderer = bombRenderer;
            if(activeRenderer) activeRenderer.material.color = bombColorStart;
        } 
        else {
            random = Random.Range(0f, 1f);
            if (random < hardRate) {
                moleType = MoleType.HardHat;
                hardHatMoleObject.SetActive(true);
                activeRenderer = hardHatMoleRenderer;
                lives = 2;
                if(activeRenderer) activeRenderer.material.color = hatColorStart;
                if (hatVisualObject != null) hatVisualObject.SetActive(true);
            } 
            else {
                moleType = MoleType.Standard;
                standardMoleObject.SetActive(true);
                activeRenderer = standardMoleRenderer;
                lives = 1;
                if(activeRenderer) activeRenderer.material.color = stdColorStart;
            }
        }
        hittable = true;
    }

    public void TryHit() {
        if (!hittable) return;

        switch (moleType) {
            case MoleType.Standard:
                if (hitParticles != null) hitParticles.Play();
                PlaySound(hitClip);
                HandleHitSuccess();
                break;

            case MoleType.HardHat:
                if (lives == 2) {
                    lives--;
                    if (hatVisualObject != null) hatVisualObject.SetActive(false);
                    PlaySound(hardHatClankClip);
                } else {
                    if (hitParticles != null) hitParticles.Play();
                    PlaySound(hitClip);
                    HandleHitSuccess();
                }
                break;

            case MoleType.Bomb:
                if (borderRenderer != null) borderRenderer.material.color = Color.red;
                if (bombParticles != null) bombParticles.Play();
                PlaySound(bombClip);
                gameManager.GameOver(1);
                hittable = false;
                break;
        }
    }

    private void HandleHitSuccess() {
        hittable = false;
        
        // Add score, but don't release the hole yet
        gameManager.AddScore(moleIndex);

        // Stop movement immediately
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);

        // Visual Feedback
        if (borderRenderer != null) {
            borderRenderer.material.color = Color.green;
        }

        if (activeRenderer != null) {
            // Darken the color to indicate a hit
            activeRenderer.material.color = activeRenderer.material.color * Color.gray;
        }

        // Start delay before hiding
        StartCoroutine(DeathDelay());
    }

    private IEnumerator DeathDelay() {
        // Wait for the specified duration to show the hit state
        yield return new WaitForSeconds(deathDelayDuration);
        
        Hide();
        
        // Notify the Game Manager that this hole is now free
        gameManager.ReleaseMole(moleIndex);
    }

    public void Hide() {
        moleMoveContainer.localPosition = startPosition;
        if (borderRenderer != null) borderRenderer.material.color = originalBorderColor;
    }

    private IEnumerator MoveUpAndDown() {
        Vector3 start = startPosition;
        Vector3 end = endPosition;

        moleMoveContainer.localPosition = start;
        if (dustParticles != null) dustParticles.Play();
        PlaySound(appearClip);

        // Move Up
        float elapsed = 0f;
        while (elapsed < showDuration) {
            moleMoveContainer.localPosition = Vector3.Lerp(start, end, elapsed / showDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        moleMoveContainer.localPosition = end;

        // Wait at top
        yield return new WaitForSeconds(duration);

        if (dustParticles != null) dustParticles.Play();

        // Move Down
        elapsed = 0f;
        while (elapsed < showDuration) {
            moleMoveContainer.localPosition = Vector3.Lerp(end, start, elapsed / showDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        moleMoveContainer.localPosition = start;

        // If missed
        if (hittable) {
            hittable = false;
            gameManager.Missed(moleIndex, moleType != MoleType.Bomb);
        }
    }

    private void PlaySound(AudioClip clip) {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }

    private void SetLevel(int level) {
        bombRate = Mathf.Min(level * 0.025f, 0.25f);
        hardRate = Mathf.Min(level * 0.025f, 1f);
        float durationMin = Mathf.Clamp(1 - level * 0.1f, 0.01f, 1f);
        float durationMax = Mathf.Clamp(2 - level * 0.1f, 0.01f, 2f);
        duration = Random.Range(durationMin, durationMax);
    }

    public void SetIndex(int index) { moleIndex = index; }
    public void StopGame() { hittable = false; StopAllCoroutines(); }
}