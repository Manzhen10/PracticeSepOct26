class TemperatureSensor
{
    public event Action<int> TemperatureChanged;

    private int now;

    public void Set(int nt)
    {
        if (nt != now)
        {
            now = nt;
            Console.WriteLine($"[СЕНСОР]: Температура изменилась на {now} градусов");
            TemperatureChanged.Invoke(now);
        }
    }
}

class Thermostat
{
    public void React(int temp)
    {
        if (temp < 16)
        {
            Console.WriteLine("[ТЕРМОСТАТ]: Стало холодно! Включение отопления");
        }
        else if (temp >= 25)
        {
            Console.WriteLine("[ТЕРМОСТАТ]: Стало жарко! Выключение отопления");
        }
        else
        {
            Console.WriteLine("[ТЕРМОСТАТ]: Нормальная температура!");
        }
    }
}

class Program
{
    static void Main()
    {
        TemperatureSensor sensor = new TemperatureSensor();
        Thermostat thermostat = new Thermostat();

        sensor.TemperatureChanged += thermostat.React;
        Console.WriteLine("Статусы:");
        sensor.Set(15); 
        sensor.Set(20); 
        sensor.Set(25); 
    }
}