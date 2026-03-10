public class UpdatedShotsSignal
{
    public float currentValue { get; private set; }

    public UpdatedShotsSignal(float score)
    {
        this.currentValue = score;
    }
}
