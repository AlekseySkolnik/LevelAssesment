namespace Patterns.Creational;
public interface IAnimal
{
    void Speak();
    void Action();
}

public class Dog : IAnimal
{
    public void Speak()
    {
        Console.WriteLine("Dog says: Bow-Wow.");
    }
    public void Action()
    {
        Console.WriteLine("Dogs prefer barking...");
    }
}

public class Tiger : IAnimal
{
    public void Speak()
    {
        Console.WriteLine("Tiger says: Halum.");
    }
    public void Action()
    {
        Console.WriteLine("Tigers prefer hunting...");
    }
}
