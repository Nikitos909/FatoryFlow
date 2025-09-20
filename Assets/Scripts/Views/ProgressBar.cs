using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;

    public void SetProgress(float progress)
    {
        fillImage.fillAmount = Mathf.Clamp01(progress);
    }
}