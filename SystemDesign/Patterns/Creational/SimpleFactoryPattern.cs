namespace Patterns.Creational;

public abstract class ISimpleFactory
{
    public abstract IAnimal CreateAnimal();
}

public class SimpleFactory : ISimpleFactory
{
    public override IAnimal CreateAnimal()
    {
        IAnimal? intendedAnimal = null;
        Console.WriteLine("Enter your choice( 0 for Dog, 1 for Tiger)");
        var b1 = Console.ReadLine();
        int input;

        if (int.TryParse(b1, out input))
        {
            Console.WriteLine("You have entered {0}", input);
            switch (input)
            {
                case 0:
                    intendedAnimal = new Dog();
                    break;
                case 1:
                    intendedAnimal = new Tiger();
                    break;
            }
        }

        return intendedAnimal;
    }
}

internal static class SimpleFactoryPattern
{
    public static void Demo()
    {
        Console.WriteLine("*** Simple Factory Pattern Demo***\n");
        var simpleFactory = new SimpleFactory();
        var preferredType = simpleFactory.CreateAnimal();

        preferredType.Speak();
        preferredType.Action();

    }
}
