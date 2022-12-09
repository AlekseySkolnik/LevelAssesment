namespace Patterns.Creational
{
    public abstract class BaseAnimalFactory
    {
        public IAnimal MakeAnimal()
        {
            IAnimal animal = CreateAnimal();
            animal.Speak();
            animal.Action();
            return animal;
        }

        public abstract IAnimal CreateAnimal();
    }

    public class DogFactory : BaseAnimalFactory
    {
        public override IAnimal CreateAnimal()
        {
            return new Dog();
        }
    }

    public class TigerFactory : BaseAnimalFactory
    {
        public override IAnimal CreateAnimal()
        {
            return new Tiger();
        }
    }

    internal static class FactoryMethodPattern
    {
        public static void Demo()
        {
            Console.WriteLine("***Factory Pattern Demo***\n");

            BaseAnimalFactory factory = new TigerFactory();
            IAnimal aTiger = factory.MakeAnimal();
            factory = new DogFactory();
            IAnimal aDog = factory.MakeAnimal();
        }
    }
}
