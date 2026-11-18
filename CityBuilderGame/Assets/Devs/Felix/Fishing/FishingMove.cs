using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingMove : MonoBehaviour
{
    public float hookStandardHeight = 200f;
    public float hookSpeed = 10f;
    public GameObject[] RopeParts = new GameObject[6];
    public int ropeDetail = 10;

    [SerializeField] private Vector2 mousePos = new Vector2(0, 0);
    [SerializeField] private Vector2 worldPos = new Vector2(0, 0);

    [SerializeField] private RectTransform hookRect;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float topOfScreen;
    [SerializeField] private RopePart baseRopePart;
    private Vector2 lastPos;
    private Vector2 posThatHookShouldBe;

    [SerializeField] private GameObject cursorCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = RopeParts.Length;
        
    }

    //krijg de muis positie
    public void MoveMouse(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        mousePos = pos;

        Debug.Log(pos);

        if(pos != lastPos)
        {
            MoveHook(pos);
        }

        lastPos = pos;
    }

    //beweeg de muis
    private void MoveHook(Vector2 pos)
    {
        //uhh zorgt ervoor dat je niet buiten het scherm kan
        //verander dit als er like borders(?) komen!!!
        if(pos.x < 0 || pos.x > 1930) return;

        posThatHookShouldBe = new Vector2(pos.x, hookStandardHeight);

        //zet de dingesen in de ropeposs dinges (jaja)
        UpdateRope(posThatHookShouldBe);
    }

    void Update()
    {
        if(!FishGameManager.Instance.isGameActive) return;

        hookRect.position = Vector2.Lerp(
            hookRect.position,
            posThatHookShouldBe,
            hookSpeed * Time.deltaTime
        );

        Vector3 p = Camera.main.ScreenToWorldPoint(hookRect.position);

        cursorCollider.transform.position = (
            new Vector3(
                p.x,
                p.y,
                0f
            )
        );

    }

    private void UpdateRope(Vector2 newValue)
    {
        baseRopePart.TransferPos(newValue);
    }

    void FixedUpdate() {
        //alleen bewegen als de game ACTIEF is
        if(!FishGameManager.Instance.isGameActive) return;

        for(int i = 0; i < RopeParts.Length; i++)
        {
            lineRenderer.SetPosition(
                i,
                RopeParts[i].transform.position
            );
        }
    }
}
