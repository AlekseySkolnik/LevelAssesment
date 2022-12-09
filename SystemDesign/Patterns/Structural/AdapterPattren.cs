namespace Patterns.Structural
{
    // Требуемый системой интерфейс
    interface IRectangle
    {
        float CalculateAreaOfRectangle();
    }

    // Класс в системе, реализующий IRectangle
    class Rectangle : IRectangle
    {
        public float Length;
        public float Width;
        public Rectangle(float l, float w)
        {
            Length = l;
            Width = w;
        }

        public float CalculateAreaOfRectangle() => Length * Width;
    }

    // Несовместимый интерфейс
    interface ITriangle
    {
        double CalculatePerimeterOfTriangle();
        double CalculateAreaOfTriangle();
    }

    // Наш класс
    class Triangle : ITriangle
    {
        public double BaseLength;
        public double Height;
        public Triangle(double b, double h)
        {
            BaseLength = b;
            Height = h;
        }

        public double CalculateAreaOfTriangle() => 0.5 * BaseLength * Height;
        public double CalculatePerimeterOfTriangle() =>
            BaseLength + Height + Math.Sqrt(BaseLength * BaseLength + Height * Height);
    }

    // Старая системао умеет работать только с IRectangle, поэтому
    // для работы с новым Triangle как с Rectangle делаем адаптер
    class TriangleAdapter : IRectangle
    {
        Triangle _triangle;//Adaptee
        public TriangleAdapter(Triangle t)
        {
            _triangle = t;
        }

        public float CalculateAreaOfRectangle() =>
            (float)_triangle.CalculateAreaOfTriangle();
    }


    internal static class AdapterPattren
    {
        public static void Demo()
        {
            Console.WriteLine("***Adapter Pattern Modified Demo***\n");

            var rectangle = new Rectangle(20, 10);
            PrintArea(rectangle);

            var triangle = new Triangle(20, 10);
            var triangleAdapter = new TriangleAdapter(triangle);
            // Передача треугольника вместо прямоугольника
            PrintArea(triangleAdapter);
        }

        // Метод не знает, что через адаптер TriangleAdapter он получает треугольник вместо прямоугольника
        private static void PrintArea(IRectangle rectangle)
        {
            var rectangleArea = rectangle.CalculateAreaOfRectangle();
            Console.WriteLine("Area of Rectangle is :{0} Square unit", rectangleArea);
        }
    }
}
