using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellPuzzleManager : MonoBehaviour
{
    public GameObject[] bells;
    public Transform[] possiblePositions;
    [SerializeField] private MMF_Player correctBellFeedbacks, incorrectBellFeedbacks, puzzleCompleteFeedbacks;
    public LockedDoor closedDoor;
    //[SerializeField] private ManageSacrifice[] manageSacrifices;

    private int currentBellIndex;
    private bool isPuzzleComplete = false;

    private void Start()
    {
        currentBellIndex = 0;

        ShufflePositions();

    }

    private void ShufflePositions()
    {
        // Shuffle the possible positions array to randomize the order
        for (int i = 0; i < possiblePositions.Length; i++)
        {
            int randomIndex = Random.Range(i, possiblePositions.Length);
            Transform temp = possiblePositions[i];
            possiblePositions[i] = possiblePositions[randomIndex];
            possiblePositions[randomIndex] = temp;
        }

        // Assign each object to a randomly-selected position
        for (int i = 0; i < bells.Length; i++)
        {
            int randomIndex = Random.Range(0, possiblePositions.Length);
            bells[i].transform.position = possiblePositions[randomIndex].position;
            // Remove the position from the array to prevent duplicates
            possiblePositions[randomIndex] = possiblePositions[possiblePositions.Length - 1];
            System.Array.Resize(ref possiblePositions, possiblePositions.Length - 1);
        }
    }

    public void RingBell(int bellIndex)
    {
        if (isPuzzleComplete) return;

        if (bellIndex == currentBellIndex + 1)
        {
            CorrectBellRinging();

            if (currentBellIndex == bells.Length)
            {
                // All bells have been rung in the correct order
                PuzzleCompleted();
            }
        }
        else
        {
            // Incorrect bell ringing, reset the puzzle
            ResetPuzzle();
        }
    }

    private void CorrectBellRinging()
    {
        foreach (var bell in bells)
        {
            var bellInfo = bell.GetComponent<CathedralBell>();
            if (bellInfo.bellIndex == currentBellIndex +1)
            {
                bellInfo.CorrectOrder();
            }
        }
        correctBellFeedbacks.PlayFeedbacks();

        currentBellIndex++;
    }

    private void PuzzleCompleted()
    {
        // TODO: Add code to execute when the puzzle is completed
        puzzleCompleteFeedbacks.PlayFeedbacks();
        isPuzzleComplete= true;
        closedDoor.canOpen = true;
    }

    private void ResetPuzzle()
    {
        incorrectBellFeedbacks.PlayFeedbacks();
        foreach (var bell in bells)
        {
            bell.GetComponent<CathedralBell>().ResetPuzzle();
        }
        Debug.Log("Incorrect order. Resetting puzzle...");
        currentBellIndex = 0;
    }
}
