class Uved
{
    public event Action<string>? Message;
    public event Action<string>? Call;
    public event Action<string>? Email;

    public void SendM(string text)
    {
        Message?.Invoke(text);
    }

    public void zvonok(string text)
    {
        Call?.Invoke(text);
    }

    public void pochta(string text)
    {
        Email?.Invoke(text);
    }
}
class Program
{
    static void Main()
    {
        Uved notification = new Uved();
        notification.Message += text =>
            Console.WriteLine("Сообщение: " + text);
        notification.Call += text =>
            Console.WriteLine("Звонок: " + text);
        notification.Email += text =>
            Console.WriteLine("Электронная почта: " + text);
        notification.SendM("Новое сообщение");
        notification.zvonok("Входящий звонок");
        notification.pochta("Новое письмо");
    }
}