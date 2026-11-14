using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrailManager : MonoBehaviour
{
    [SerializeField] Transform target;

    public List<GameObject> fireTrailObjects, iceTrailObjects, poisonTrailObjects, electricityTrailObjects;
    public float spawnInterval = 1f;
    public float fadeDuration = 0.1f;
    public GlobalStats globalStats;

    private void Start()
    {
        //foreach (var fireObj in fireTrailObjects)
        //{
        //    fireObj.transform.parent = null;
        //}

        PlaceObjectsAtPlayerPosition(fireTrailObjects);


        //switch (globalStats.walkTrailDamageType)
        //{
        //    case DamageType.Puncture:
        //        break;
        //    case DamageType.Slash:
        //        break;
        //    case DamageType.Impact:
        //        break;
        //    case DamageType.Explosive:
        //        break;
        //    case DamageType.Fire:
        //        PlaceObjectsAtPlayerPosition(fireTrailObjects);
        //        break;
        //    case DamageType.Ice:
        //        PlaceObjectsAtPlayerPosition(iceTrailObjects);
        //        break;
        //    case DamageType.Poison:
        //        PlaceObjectsAtPlayerPosition(poisonTrailObjects);
        //        break;
        //    case DamageType.Water:
        //        break;
        //    case DamageType.Electricity:
        //        PlaceObjectsAtPlayerPosition(electricityTrailObjects);
        //        break;
        //    case DamageType.Ether:
        //        break;
        //    case DamageType.None:
        //        break;
        //    default:
        //        break;
        //}

    }


    public void PlaceObjectsAtPlayerPosition(List<GameObject> objects)
    {
        //Detach so it no longer follows the player.
        foreach (var item in objects)
        {
            item.transform.parent = null;
        }

        StartCoroutine(PlaceObjectsCoroutine(objects));
    }

    //private IEnumerator PlaceObjectsCoroutine(List<GameObject> objects)
    //{
    //    int index = 0;
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(0.1f);

    //        objects[index].transform.position = target.position;
    //        index = (index + 1) % objects.Count;
    //        yield return new WaitForSeconds(spawnInterval); // Adjust the interval as needed.
    //    }
    //}

    private IEnumerator PlaceObjectsCoroutine(List<GameObject> objects)
    {
        int index = 0;
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            GameObject obj = objects[index];
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Fade out
                yield return StartCoroutine(FadeAlpha(sr, 1f, 0f, fadeDuration));
            }

            obj.transform.position = target.position;

            if (sr != null)
            {
                // Fade in
                yield return StartCoroutine(FadeAlpha(sr, 0f, 1f, fadeDuration));
            }

            index = (index + 1) % objects.Count;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator FadeAlpha(SpriteRenderer sr, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color color = sr.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            sr.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        sr.color = new Color(color.r, color.g, color.b, to);
    }
}
