using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * source: http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html 
 *
 * This script will be used for scaling target aspect ratio for fitting on every screen and device.
 *
 * "Auto Letterbox" Asset will be considered to use in the future.
 */


public class CameraAspectRatioFix : MonoBehaviour
{
    public float targetWidth;
    public float targetHeight;
    void Start () 
    {
        // the desired aspect ratio
        float targetaspect = targetWidth / targetHeight;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        float scaleheight = windowaspect / targetaspect;

        // obtain camera component so we can modify its viewport
        Camera camera = GetComponent<Camera>();

        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {  
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;
        
            camera.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }
}
