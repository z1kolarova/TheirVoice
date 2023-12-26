using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class ScreenControl : MonoBehaviour
{
    VideoPlayer _videoPlayer;

    [SerializeField]
    int delaySeconds = 10;

    // Start is called before the first frame update
    void Start()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.frame = (int)_videoPlayer.frameRate * delaySeconds;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
