namespace Patterns.Structural.ProxyPattern
{
    public abstract class AbstractSubject
    {
        public abstract void DoSomeWork();
    }

    public class ConcreteSubject : AbstractSubject
    {
        public override void DoSomeWork()
        {
            Console.WriteLine("ConcreteSubject.DoSomeWork()");
        }
    }

    public class Proxy : AbstractSubject
    {
        private AbstractSubject? _concreteSubject;

        public override void DoSomeWork()
        {
            Console.WriteLine("Proxy call happening now...");

            //Lazy initialization:We'll not instantiate until the method is called
            _concreteSubject ??= new ConcreteSubject();
            _concreteSubject.DoSomeWork();
        }
    }

    internal static class ProxyPattern
    {
        public static void Demo()
        {
            Console.WriteLine("***Proxy Pattern Demo***\n");
            var px = new Proxy();
            px.DoSomeWork();
        }
    }
}
