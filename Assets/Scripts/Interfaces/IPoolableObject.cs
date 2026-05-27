public interface IPoolableObject : IPrototype
{
    public bool Active //Quizás lo puedas borrar
    {
        get;
        set;
    }

    public void ResetObject();
}
