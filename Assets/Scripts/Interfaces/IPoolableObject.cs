public interface IPoolableObject : IPrototype
{
    public bool Active 
    {
        get;
        set;
    }

    public void ResetObject();
}
