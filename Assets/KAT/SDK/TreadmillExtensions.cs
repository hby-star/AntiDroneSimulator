using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreadmillExtensions : MonoBehaviour
{
    [Range(0.5f, 5.0f)]
    public float lerpSpeed = 1.0f;

    private float tmpSpeed = 0.0f;

    bool atten = false;

    // Update is called once per frame
    void Update()
    {
        //Press R to reload scene
        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }

        //Press and Release L Key to bright LED Once
        if (Input.GetKeyUp(KeyCode.L))
        {
            KATNativeSDK.KATExtension.LEDOnce(1.0f);
        }

        //Press and Release L Key to vibrate once
        if (Input.GetKeyUp(KeyCode.V))
        {
            KATNativeSDK.KATExtension.VibrateOnce(1.0f);
        }

        //Press J to let LED breath once
        if (Input.GetKey(KeyCode.J))
        {
            tmpSpeed += Time.deltaTime / lerpSpeed;
            if (tmpSpeed > 1.0f)
            {
                tmpSpeed = 1.0f;
            }
            KATNativeSDK.KATExtension.LEDConst(tmpSpeed);
            atten = true;
        }
        else
        {
            if (atten)
            {
                tmpSpeed -= Time.deltaTime / lerpSpeed;
                if (tmpSpeed < 0.0f)
                {
                    tmpSpeed = 0.0f;
                    atten = false;
                }
                KATNativeSDK.KATExtension.LEDConst(tmpSpeed);
            }

        }
    }
}
