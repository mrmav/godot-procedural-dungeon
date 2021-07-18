using Godot;
using System;

public class Damage : Node
{
    public int Amount;
    public Node Agressor;
    public Node Defender;
    public Vector2 Normal;
    public int Knockback;

    public Damage(int amount = 0, int knockback = 100, Node agressor = null, Node defender = null, float normalX = 0, float normalY = 0)
    {
        Amount = amount;
        Knockback = knockback;
        Agressor = agressor;
        Defender = defender;
        Normal = new Vector2(normalX, normalY);
    }
}