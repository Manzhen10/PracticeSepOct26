using System.Diagnostics;

delegate void Sort(int[] nums);

class Program
{
    static void Main()
    {
        int[] nums;

        while (true)
        {
            Console.Write("Введите числа через пробел: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Введите числа!");
                continue;
            }

            string[] parts = input.Split(' ');

            int[] temp = new int[parts.Length];
            bool valid = true;

            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out temp[i]))
                {
                    Console.WriteLine("Введите числа через пробел без букв и символов!");
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;

            nums = temp;
            break;
        }

        Sort sort;

        while (true)
        {
            Console.WriteLine("\nВыберите сортировку:");
            Console.WriteLine("1. Пузырьковая сортировка");
            Console.WriteLine("2. Быстрая сортировка");
            Console.WriteLine("3. Сортировка вставками");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    sort = BubbleSort;
                    break;

                case "2":
                    sort = QuickSort;
                    break;

                case "3":
                    sort = InsertionSort;
                    break;

                default:
                    Console.WriteLine("Неверный выбор! Попробуйте снова.\n");
                    continue;
            }

            break;
        }

        Stopwatch timer = Stopwatch.StartNew();

        sort(nums);

        timer.Stop();

        Console.WriteLine("\nРезультат: " + string.Join(", ", nums));
        Console.WriteLine($"Время сортировки: {timer.Elapsed.TotalMilliseconds:F6} мс");
    }

    // Пузырьковая сортировка
    static void BubbleSort(int[] nums)
    {
        for (int i = 0; i < nums.Length - 1; i++)
        {
            for (int j = 0; j < nums.Length - i - 1; j++)
            {
                if (nums[j] > nums[j + 1])
                {
                    int temp = nums[j];
                    nums[j] = nums[j + 1];
                    nums[j + 1] = temp;
                }
            }
        }
    }

    // Быстрая сортировка
    static void QuickSort(int[] nums)
    {
        QuickSort(nums, 0, nums.Length - 1);
    }

    static void QuickSort(int[] nums, int left, int right)
    {
        if (left >= right)
            return;

        int pivot = nums[(left + right) / 2];
        int i = left;
        int j = right;

        while (i <= j)
        {
            while (nums[i] < pivot)
                i++;

            while (nums[j] > pivot)
                j--;

            if (i <= j)
            {
                int temp = nums[i];
                nums[i] = nums[j];
                nums[j] = temp;

                i++;
                j--;
            }
        }

        if (left < j)
            QuickSort(nums, left, j);

        if (i < right)
            QuickSort(nums, i, right);
    }

    // Сортировка вставками
    static void InsertionSort(int[] nums)
    {
        for (int i = 1; i < nums.Length; i++)
        {
            int key = nums[i];
            int j = i - 1;

            while (j >= 0 && nums[j] > key)
            {
                nums[j + 1] = nums[j];
                j--;
            }

            nums[j + 1] = key;
        }
    }
}