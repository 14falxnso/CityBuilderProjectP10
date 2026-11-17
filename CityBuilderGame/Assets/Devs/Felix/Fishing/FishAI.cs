using NUnit.Framework;
using UnityEngine;

public class FishAI : MonoBehaviour
{
    [SerializeField] private Vector2 startPos;
    [SerializeField] private bool isFlipped = false;
    [SerializeField] private float speed = 0;
    [SerializeField] private float speedDown = 5;

    [Header("[0] = minimum, [1] = maximum")]
    public float[] speedRange = new float[2]{2f, 5.5f};
    [Header("(multiplier!!!) [0] = minimum, [1] = maximum")]
    public float[] sizeRange = new float[2]{0.9f, 1.1f};
    private int flipFactor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //bepaald start pos
        startPos = new Vector3(
            Random.Range(20f, 1900f),
            1000f,
            transform.position.z
        );

        //willekeurige snelheid, wany niet alle vissen zijn even snel!
        speed = Random.Range(
            speedRange[0],
            speedRange[1]
        );


        //BEETJE willekeurige grootte hha
        Vector3 curScale = transform.localScale;

        float scaleFactor = Random.Range(
            sizeRange[0],
            sizeRange[1]
        );

        transform.localScale = curScale * scaleFactor;

        transform.position = startPos;

        //check of de vis aan de ene kant staat en dan uhh spiegel that thang
        if(startPos.x > (20f + 1900f) / 2 && Random.Range(0, 10) > 3)
        {
            Vector3 s = transform.localScale;

            //flip
            transform.localScale = new Vector3(
                -s.x,
                s.y,
                s.z
            );

            isFlipped = true;
            flipFactor = -1;
        }
        else
        {
            flipFactor = 1;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //animatie soort dinges!
        float angle = Mathf.Sin(Time.time * 10f) * 5f;
        transform.localRotation = Quaternion.Euler(0f, angle, 0f);

        //die ene flip factor zorgt voor goede bewegin!
        transform.position -= Vector3.up * speedDown + Vector3.left * speed * flipFactor;


        //zorgt ervoor dat ie dood wordt gemaakt als hij aan de onderkant zit
        if(transform.position.y <= -500)
        {
            Destroy(gameObject);
        }
    }
}
