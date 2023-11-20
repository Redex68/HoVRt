using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TiltTooltip : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text displayText;
    [SerializeField] GameObject progressBar;
    [SerializeField] Color delayColour;
    [SerializeField] Color readingColour;

    private UnityEngine.UI.Image fill;
    void Start()
    {
        fill = progressBar.transform.Find("Fill").GetComponent<UnityEngine.UI.Image>();
    }

    public void OnCalibrationEvent(Component sender, object data)
    {
        CalibrationEvent calibrationEvent = (CalibrationEvent) data;
        Tilt tilt = (Tilt) sender;

        switch(calibrationEvent)
        {
            case CalibrationEvent.LEFT:
            case CalibrationEvent.RIGHT:
            case CalibrationEvent.DOWN:
                progressBar.SetActive(true);
                break;
            case CalibrationEvent.FINISHED:
                progressBar.SetActive(false);
                break;
            default:
                Debug.LogError("Unsupported CalibrationEvent");
                return;
        }

        switch(calibrationEvent)
        {
            case CalibrationEvent.LEFT:
                progressBar.SetActive(true);
                displayText.text = "Tilt left";
                StartCoroutine(TooltipUpdater("Hold", tilt.readDelay, tilt.readLength));
                break;
            case CalibrationEvent.RIGHT:
                displayText.text = "Tilt right";
                StartCoroutine(TooltipUpdater("Hold", tilt.readDelay, tilt.readLength));
                break;
            case CalibrationEvent.DOWN:
                displayText.text = "Lay down flat";
                StartCoroutine(TooltipUpdater("Hold", tilt.readDelay, tilt.readLength));
                break;
            case CalibrationEvent.FINISHED:
                displayText.text = "Calibrated";
                break;
        }
    }

    private IEnumerator TooltipUpdater(string newText, float readDelay, float readLength)
    {
        fill.color = delayColour;
        StartCoroutine(UpdateProgress(readDelay));
        yield return new WaitForSeconds(readDelay);

        displayText.text = newText;
        fill.color = readingColour;
        StartCoroutine(UpdateProgress(readLength));
    }

    private IEnumerator UpdateProgress(float time)
    {
        float startTime = Time.time;
        float endTime = Time.time + time;
        while(Time.time < endTime)
        {
            fill.fillAmount = Mathf.InverseLerp(startTime, endTime, Time.time);
            yield return new WaitForNextFrameUnit();
        }
        fill.fillAmount = 1.0f;
    }
}
