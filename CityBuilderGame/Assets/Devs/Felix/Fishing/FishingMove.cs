using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingMove : MonoBehaviour
{
    public float hookStandardHeight = 200f;
    public float hookSpeed = 10f;
    public Vector2[] ropePosses;
    public int ropeDetail = 10;
    [SerializeField] private Vector2 mousePos = new Vector2(0, 0);
    [SerializeField] private RectTransform hookRect;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float topOfScreen;
    private Vector2 lastPos;
    private Vector2 posThatHookShouldBe;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        ropePosses = new Vector2[ropeDetail];
        lineRenderer.positionCount = ropeDetail;
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
        UpdateRopeArray(posThatHookShouldBe);
    }

    void Update()
    {
        hookRect.position = Vector2.Lerp(
            hookRect.position,
            posThatHookShouldBe,
            hookSpeed * Time.deltaTime
        );
    }

    private void UpdateRopeArray(Vector2 newValue)
    {
        for(int i = ropePosses.Length - 1; i > 0; i--)
        {
            ropePosses[i] = new Vector2(ropePosses[i - 1].x, ((topOfScreen - hookRect.position.y) / ropeDetail) * i);
        }

        ropePosses[0] = newValue;
    }
}
