public struct  UpdatedShotsEvent
{
    public float currentValue { get; private set; }

    public UpdatedShotsEvent(float score)
    {
        this.currentValue = score;
    }
}
