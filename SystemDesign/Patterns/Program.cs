using Patterns.Creational;
using Patterns.Creational.AbstractFactoryPattern;
using Patterns.Structural;
using Patterns.Structural.DecoratorPattern;
using Patterns.Structural.ProxyPattern;


Console.WriteLine("Creational Patterns");
//SimpleFactoryPattern.Demo();
FactoryMethodPattern.Demo();
AbstractFactoryPattern.Demo();

Console.WriteLine("Structural Patterns");
ProxyPattern.Demo();
DecoratorPattern.Demo();
AdapterPattren.Demo();
FacadePattern.Demo();
FlyweightPattern.Demo();
BridgePattern.Demo();

Console.ReadLine();