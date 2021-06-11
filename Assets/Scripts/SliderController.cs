using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public enum SliderResult
    {
        NONE,
        BOARD_X,
        BOARD_Y,
        START,
    }

    public SliderResult Result;
    public Text SliderText;
    private Slider ConnectedSlider;

    private void Start()
    {
        ConnectedSlider = GetComponent<Slider>();

        switch (Result)
        {
            case SliderResult.NONE:
                break;

            case SliderResult.BOARD_X:
                ConnectedSlider.value = GameManager.SetMaxX;
                break;

            case SliderResult.BOARD_Y:
                ConnectedSlider.value = GameManager.SetMaxY;
                break;

            case SliderResult.START:
                ConnectedSlider.value = GameManager.StartingNodeCount;
                break;

            default:
                break;
        }

        SliderText.text = ConnectedSlider.value.ToString();
    }

    public void UpdateValue(float Value)
    {
        SliderText.text = Value.ToString();

        switch (Result)
        {
            case SliderResult.NONE:
                break;

            case SliderResult.BOARD_X:
                GameManager.SetMaxX = (int)Value;
                break;

            case SliderResult.BOARD_Y:
                GameManager.SetMaxY = (int)Value;
                break;

            case SliderResult.START:
                GameManager.StartingNodeCount = (int)Value;
                break;

            default:
                break;
        }
    }
}
