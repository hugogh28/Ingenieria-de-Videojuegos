public interface IObjectPool
{
    public IPoolableObject Get();
    public void Release(IPoolableObject o);
}