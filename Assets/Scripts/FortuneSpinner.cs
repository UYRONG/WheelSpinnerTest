using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FortuneSpinner : MonoBehaviour
{
    public Image FortuneWheelBase;
    public Image[] RewardGroup;

    public GameObject PlayButtom;
    public GameObject ClaimButtom;
    private Coroutine rewardCoroutine;
    Dictionary<int, int> PricesWithWeights = new()
    {
        {0, 20}, // Hammer 1x
        {1, 10}, // Gem 75
        {2, 10}, // Brush 1x
        {3, 10}, // Coin 750
        {4, 5},  // Hammer 3x
        {5, 20}, // Gem 35
        {6, 5},  // Brush 3x
        {7, 20}  // Life 30
    };

    public int MinRotations = 2;
    public int MaxRotations = 6;
    public float SpinDuration = 5f;

    private bool isSpinning;
    private float anglePerItem;
    private float targetAngle;
    private float currentAngle;
    private float elapsedTime;
    private bool showReward;
    private int rewardIndex;
    private bool isWaiting = false;

    public Coroutine waitCoroutine = null;

    private void Start()
    {
        isSpinning = false;
        anglePerItem = 360f / PricesWithWeights.Count;
        // Simulating 1000 result function 
        EmulateSpins();
    }

    private void Update()
    {
        if (isSpinning)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Min(elapsedTime / SpinDuration, 1f);

            // Use easing out for smooth deceleration process of the spine
            float easedProgress = 1 - Mathf.Pow(1 - progress, 3);
            currentAngle = Mathf.Lerp(0, targetAngle, easedProgress);

            FortuneWheelBase.transform.localRotation = Quaternion.Euler(0, 0, -currentAngle);

            if (progress >= 1f)
            {
                isSpinning = false;
                SetShowReward();
            }
        }
        if (showReward && rewardIndex != -1)
        {
            RewardGroup[rewardIndex].transform.localScale = (1 + 0.2f * Mathf.Sin(10 * Time.time)) * Vector3.one;
        }

        // quite application for mac build
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

    public void StartSpin()
    {
        // Not allowed to click if the wheel is spinning
        if (isSpinning) return;

        isSpinning = true;
        elapsedTime = 0;

        // Determine the reward index based on weights/probability
        rewardIndex = GetRandomWeightedIndex();

        // Calculate the target angle to stop on the selected reward index
        targetAngle = CalculateTargetAngle(rewardIndex);

        Debug.Log($"Spinning to reward index {rewardIndex},  target angle {targetAngle}");
    }
    private int GetRandomWeightedIndex()
    {
        int totalWeight = 0;
        foreach (KeyValuePair<int, int> kvp in PricesWithWeights)
        {
            totalWeight += kvp.Value;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var kvp in PricesWithWeights)
        {
            cumulativeWeight += kvp.Value;
            if (randomValue < cumulativeWeight)
            {
                // Return the key/index of the selected reward
                return kvp.Key;
            }
        }

        return 0; // Fallback if errors
    }

    // Calculate the target rotation angle for the selected reward index
    private float CalculateTargetAngle(int rewardIndex)
    {
        float baseAngle = rewardIndex * anglePerItem;
        int fullRotations = UnityEngine.Random.Range(MinRotations, MaxRotations);
        // Add a random offset within the segment range
        float randomOffset = UnityEngine.Random.Range(0f, anglePerItem);
        return baseAngle + randomOffset + (fullRotations * 360f);
    }

    public void EmulateSpins(int spinCount = 1000, bool outputToFile = false)
    {
        // Dictionary to store the count of each spine result <reward index, count>
        Dictionary<int, int> results = new Dictionary<int, int>();

        foreach (var kvp in PricesWithWeights)
        {
            results[kvp.Key] = 0;
        }

        // Simulate the spins and record the results
        for (int i = 0; i < spinCount; i++)
        {
            int prizeIndex = GetRandomWeightedIndex();
            results[prizeIndex]++;
        }

        // Format the output as a string
        string output = "Spin Results (Grouped by Prize):\n";
        foreach (var kvp in results)
        {
            Debug.Log($"Prize {kvp.Key}: {kvp.Value} spins\n");
            output += $"Prize {kvp.Key}: {kvp.Value} spins\n";
        }

        // Output to console or file based on parameter
        if (outputToFile)
        {
            string path = "SpinResults.txt";
            File.WriteAllText(path, output);
            Debug.Log($"Results written to file: {path}");
        }
    }
    private IEnumerator WaitForSomeSeconds()
    {
        isWaiting = true;
        float WaitSec = SpinDuration + 1f;
        yield return new WaitForSeconds(WaitSec);
        isWaiting = false;
        waitCoroutine = null; // Clear the coroutine reference when done
    }

    // Method to safely start the waiting coroutine
    public void StartWaitForSomeSeconds()
    {
        // Stop the existing coroutine if it's running
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
        }

        // Start the coroutine and store the reference
        if (!isSpinning && waitCoroutine == null)
        {
            waitCoroutine = StartCoroutine(WaitForSomeSeconds());
        }
    }
    private void SetShowReward()
    {
        // RewardPanel.SetActive(true);
        StartCoroutine(WaitForSomeSeconds());
        ClaimButtom.SetActive(true);
        PlayButtom.SetActive(false);
        showReward = true;
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
        }
    }

    public void ClaimAndBack()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
        }
        // reset icon size
        RewardGroup[rewardIndex].transform.localScale = Vector3.one;
        showReward = false;
        isSpinning = false;
        currentAngle = 0;
        rewardIndex = -1;
        ClaimButtom.SetActive(false);
        PlayButtom.SetActive(true);
        FortuneWheelBase.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Debug.Log("Fortune wheel rotation reset.");
    }
}
