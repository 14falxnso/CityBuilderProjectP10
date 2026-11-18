using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RopePart : MonoBehaviour
{
    private RectTransform rect;
    private Vector2 posToBe;
    [SerializeField] private RopePart nextPart;
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }
    public void TransferPos(Vector2 pos)
    {
        pos = new Vector2(pos.x, rect.position.y);
        StartCoroutine(WaitForNext(pos));
        posToBe = pos;
    } 

    private IEnumerator WaitForNext(Vector2 pos)
    {
        yield return new WaitForSeconds(0.1f);

        if (nextPart)
        {
            nextPart.TransferPos(pos);
        }
    }

    void Update()
    {
        rect.position = new Vector2(Vector2.LerpUnclamped(
            rect.position,
            posToBe,
            10f * Time.deltaTime
        ).x, rect.position.y);
    }
}
