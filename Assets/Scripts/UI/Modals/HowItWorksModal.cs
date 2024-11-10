public class HowItWorksModal : JustCloseModal
{
    public static HowItWorksModal I => instance;
    static HowItWorksModal instance;

    protected override void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }
}
