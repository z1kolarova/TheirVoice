public static class CrossPlatformUtils
{
    public static void SetTextToClipboard(string text)
    {
#if UNITY_STANDALONE_WIN
        System.Windows.Forms.Clipboard.SetText(text);
#elif UNITY_WEBGL
#endif
    }
}
