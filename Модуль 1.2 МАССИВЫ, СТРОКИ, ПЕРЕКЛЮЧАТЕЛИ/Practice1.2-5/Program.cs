class Program
{
    static void Main()
    {
        Console.Write("Введите количесвто элементов k: ");
        int k = Convert.ToInt32 (Console.ReadLine());
        string alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        Random r = new Random();
        char[] bukvi = new char[k];
  
        for (int i = 0; i < bukvi.Length; i++)
        {
            int index = r.Next(alphabet.Length); 
            bukvi[i] = alphabet[index];      
        }
        Console.WriteLine("Массив: " + String.Join(",", bukvi));
        char[] sogl = new char[] {'Б', 'В', 'Г', 'Д', 'Ж', 'З', 'К', 'Л', 'М', 'Н', 'П', 'Р', 'С', 'Т', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'б', 'в', 'г', 'д', 'ж', 'з', 'к', 'л', 'м', 'н', 'п', 'р', 'с', 'т', 'ф', 'х', 'ц', 'ч', 'ш', 'щ' };
        
        char[] obsch = bukvi.Intersect(sogl).ToArray();
        Console.WriteLine("Только согласные: " + String.Join(",", obsch));

        
    }
}