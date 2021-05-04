using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultDialog : MonoBehaviour
{
    public float DialogShowTime = 2.0f;

    float timePassed = 0.0f;

    public delegate void OnDialogFinished_Delegate();

    public OnDialogFinished_Delegate OnDialogFinished;

    public void ShowDialog(string text)
    {
        timePassed = 0.0f;
        transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = text;
    }

    // Update is called once per frame
    void Update()
    {
        if (timePassed >= DialogShowTime)
        {
            if (OnDialogFinished != null)
                OnDialogFinished();

            gameObject.SetActive(false);
        }

        timePassed += Time.deltaTime;
    }
}
