using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    public Image NodeImage;
    public Text NodeText;

    public Vector2 NodePosition;
    public int score = 2;
    public int ColorIndex = 0;

    public bool Kill;

    private Vector2 FinalPosition;

    public Node(Vector2 POS)
    {
        NodePosition = POS;
    }

    public void SetNodePhysicalPosition(Vector2 newPos)
    {
        FinalPosition = newPos;
    }

    public void SetNodeScore()
    {
        score *= 2;
        ColorIndex++;

        GameManager.Instance.AddScore(score);

        if (score == 2048) GameManager.Instance.Win();

        NodeText.text = score.ToString();
        NodeImage.color = GameManager.Instance.ColorList[ColorIndex];
    }

    private void Start()
    {
        FinalPosition = GetComponent<RectTransform>().localPosition;
        GetComponent<RectTransform>().localScale = Vector3.one * 3f;
    }

    private void Update()
    {
        RectTransform myPOS = GetComponent<RectTransform>();
        myPOS.localPosition = Vector2.Lerp(myPOS.localPosition, FinalPosition, 50 * Time.deltaTime);

        myPOS.localScale = Vector2.Lerp(myPOS.localScale, Vector3.one, 50*Time.deltaTime);

        if(Kill && Vector2.Distance(myPOS.localPosition, FinalPosition) < 10)
        {
            Destroy(gameObject);
        }
    }
}
