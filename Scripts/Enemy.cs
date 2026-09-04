using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public int Health = 3;

	public void TakeDamage(int damage)
	{
		Health -= damage;

		GD.Print("Enemy terkena damage! HP: " + Health);

		if (Health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print("Enemy kalah!");

		QueueFree();
	}
}
