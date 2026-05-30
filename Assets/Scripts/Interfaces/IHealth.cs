public interface IHealth
{
    public float health { get; set; }
    public void TakeDamage(float damage)
    {
        health -= damage;
        
        if(health <= 0)
        {
            Die();
        }

        else{

        }
    }

    public void Die()
    {

    }
}
