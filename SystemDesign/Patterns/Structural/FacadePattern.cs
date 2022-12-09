namespace Patterns.Structural
{
    public class RobotHands
    {
        public void SetMilanoHands()
        {
            Console.WriteLine(" The robot will have EH1 Milano hands");
        }
        public void SetRobonautHands()
        {
            Console.WriteLine(" The robot will have Robonaut hands");
        }
        public void ResetMilanoHands()
        {
            Console.WriteLine(" EH1 Milano hands are about to be destroyed");
        }
        public void ResetRobonautHands()
        {
            Console.WriteLine(" Robonaut hands are about to be destroyed");
        }
    }

    public class RobotColor
    {
        public void SetDefaultColor()
        {
            Console.WriteLine(" This is steel color robot.");
        }
        public void SetGreenColor()
        {
            Console.WriteLine(" This is a green color robot.");
        }
    }

    public class RobotBody
    {
        public void CreateHands()
        {
            Console.WriteLine(" Hands manufactured");
        }
        public void CreateRemainingParts()
        {
            Console.WriteLine(" Remaining parts (other than hands) are created");
        }
        public void DestroyHands()
        {
            Console.WriteLine(" The robot's hands are destroyed");
        }
        public void DestroyRemainingParts()
        {
            Console.WriteLine(" The robot's remaining parts are destroyed");
        }
    }

    // Фасадный класс, который прячет детали реализации подсистемы от клиентов
    public class RobotFacade
    {
        RobotColor color;
        RobotHands hands;
        RobotBody body;
        public RobotFacade()
        {
            color = new RobotColor();
            hands = new RobotHands();
            body = new RobotBody();
        }
        public void ConstructMilanoRobot()
        {
            Console.WriteLine("Creation of a Milano Robot Start");
            color.SetDefaultColor();
            hands.SetMilanoHands();
            body.CreateHands();
            body.CreateRemainingParts();
            Console.WriteLine("Milano Robot Creation End");
            Console.WriteLine();
        }
        public void ConstructRobonautRobot()
        {
            Console.WriteLine("Initiating the creational process of a Robonaut Robot");
            color.SetGreenColor();
            hands.SetRobonautHands();
            body.CreateHands();
            body.CreateRemainingParts();
            Console.WriteLine("A Robonaut Robot is created");
            Console.WriteLine();
        }
        public void DestroyMilanoRobot()
        {
            Console.WriteLine("Milano Robot's destruction process is started");
            hands.ResetMilanoHands();
            body.DestroyHands();
            body.DestroyRemainingParts();
            Console.WriteLine("Milano Robot's destruction process is over");
            Console.WriteLine();
        }
        public void DestroyRobonautRobot()
        {
            Console.WriteLine("Initiating a Robonaut Robot's destruction process.");
            hands.ResetRobonautHands();
            body.DestroyHands();
            body.DestroyRemainingParts();
            Console.WriteLine("A Robonaut Robot is destroyed");
            Console.WriteLine();
        }

    }

    internal class FacadePattern
    {
        public static void Demo()
        {
            Console.WriteLine("***Facade Pattern Demo***\n");
            // Клиент фасада, который работает с фасадом, а не с классами подсистемы.
            var rf1 = new RobotFacade();
            rf1.ConstructMilanoRobot();

            var rf2 = new RobotFacade();
            rf2.ConstructRobonautRobot();

            rf1.DestroyMilanoRobot();
            rf2.DestroyRobonautRobot();
        }
    }
}
