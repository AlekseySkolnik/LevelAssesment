namespace Patterns.Structural.DecoratorPattern
{
    abstract class AbstractComponent
    {
        public abstract void MakeHouse();
    }

    class ConcreteComponent : AbstractComponent
    {
        public override void MakeHouse()
        {
            // It is closed for modification
            Console.WriteLine("Base functionality from ConcreteComponent");
        }
    }

    abstract class AbstractDecorator : AbstractComponent
    {
        protected AbstractComponent _component;

        public AbstractDecorator(AbstractComponent c)
        {
            _component = c;
        }

        public override void MakeHouse()
        {
            _component?.MakeHouse();//Delegating the task
        }
    }

    class ConcreteDecoratorEx1 : AbstractDecorator
    {
        public ConcreteDecoratorEx1(AbstractComponent c) : base(c) { }

        public override void MakeHouse()
        {
            Console.WriteLine("[BEFORE] Additional functionality from ConcreteDecoratorEx1");
            base.MakeHouse();
            Console.WriteLine("[AFTER] Additional functionality from ConcreteDecoratorEx1");
        }
    }

    class ConcreteDecoratorEx2 : AbstractDecorator
    {
        public ConcreteDecoratorEx2(AbstractComponent c) : base(c) { }

        public override void MakeHouse()
        {
            Console.WriteLine("[BEFORE] Additional functionality from ConcreteDecoratorEx2");
            base.MakeHouse();
            Console.WriteLine("[AFTER] Additional functionality from ConcreteDecoratorEx2");
        }
    }

    internal static class DecoratorPattern
    {
        public static void Demo()
        {
            Console.Clear();
            Console.WriteLine("***Decorator pattern Demo***\n");

            AbstractComponent abstractComponent =
                new ConcreteDecoratorEx2(
                    new ConcreteDecoratorEx1(
                        new ConcreteComponent()));

            abstractComponent.MakeHouse();
        }
    }
}
