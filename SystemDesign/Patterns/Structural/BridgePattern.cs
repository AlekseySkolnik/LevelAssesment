namespace Patterns.Structural
{
    //Implementor
    public interface IState
    {
        void MoveState();
    }

    public class OnState : IState
    {
        public void MoveState() => Console.Write("On State");
    }

    public class OffState : IState
    {
        public void MoveState() => Console.Write("Off State");
    }

    //Abstraction
    public abstract class ElectronicGoods
    {
        //Composition - implementor
        public IState State { get; set; }

        public ElectronicGoods(IState state)
        {
            State = state;
        }

        abstract public void MoveToCurrentState();
    }

    public class Television : ElectronicGoods
    {
        public Television(IState state) : base(state) { }

        public override void MoveToCurrentState()
        {
            Console.Write("\n Television is functioning at : ");
            State.MoveState();
        }
    }

    public class VCD : ElectronicGoods
    {
        public VCD(IState state) : base(state) { }
        public override void MoveToCurrentState()
        {
            Console.Write("\n VCD is functioning at : ");
            State.MoveState();
        }
    }

    internal static class BridgePattern
    {
        public static void Demo()
        {
            Console.WriteLine("***Bridge Pattern Demo***");
            Console.WriteLine("\n Dealing with a Television:");

            IState presentState = new OnState();
            ElectronicGoods eItem = new Television(presentState);
            eItem.MoveToCurrentState();

            presentState = new OffState();
            eItem.State = presentState;
            eItem.MoveToCurrentState();

            // Теперь можно добавить любой новый State, Television не изменится
        }
    }
}
