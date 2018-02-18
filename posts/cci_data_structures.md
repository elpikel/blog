# Cracking the Coding Interview Challenges - Data Structures - solutions

1. [Arrays: Left Rotation](https://www.hackerrank.com/challenges/ctci-array-left-rotation/problem)

```csharp
public class ArrayEx
{
    public string LeftRotation(int n, int k, int[] a)
    {
        var split = k % n;
        var results = new int[n];

        for (int i = split, j = 0; i < n; i++, j++)
        {
            results[j] = a[i];
        }
        for (int i = n - split, j = 0; i < n; i++, j++)
        {
            results[i] = a[j];
        }

        return string.Join(' ', results);
    }
}
```

2. [Strings: Making Anagrams](https://www.hackerrank.com/challenges/ctci-making-anagrams/problem)

```csharp
class Anagrams
{
    public int LettersToRemove(string a, string b)
    {
        var toRemove = 0;
        var aDict = CreateDict(a);
        var bDict = CreateDict(b);

        foreach (var letter in aDict)
        {
            if (letter.Value == bDict[letter.Key])
            {
                continue;
            }

            toRemove = toRemove + Math.Abs(letter.Value - bDict[letter.Key]);
        }

        return toRemove;
    }

    private Dictionary<char, int> CreateDict(string a)
    {
        var dict = new Dictionary<char, int>
        {
            {'a', 0},
            {'b', 0},
            {'c', 0},
            {'d', 0},
            {'e', 0},
            {'f', 0},
            {'g', 0},
            {'h', 0},
            {'i', 0},
            {'j', 0},
            {'k', 0},
            {'l', 0},
            {'m', 0},
            {'n', 0},
            {'o', 0},
            {'p', 0},
            {'q', 0},
            {'r', 0},
            {'s', 0},
            {'t', 0},
            {'u', 0},
            {'v', 0},
            {'w', 0},
            {'x', 0},
            {'y', 0},
            {'z', 0}
        };
        var letters = a.ToCharArray();

        foreach (var letter in letters)
        {
            dict[letter] = dict[letter] + 1;
        }

        return dict;
    }
}
```

3. [Hash Tables: Ransom Note](https://www.hackerrank.com/challenges/ctci-ransom-note/problem)

```csharp
class Ransom
{
    public string CanCreate(int m, int n, string[] magazine, string[] ransom)
    {
        if (m < n)
        {
            return "No";
        }

        var magazineDict = CreateDict(magazine);
        var ransomDict = CreateDict(ransom);

        foreach (var ransomWord in ransomDict)
        {
            if (!magazineDict.ContainsKey(ransomWord.Key))
            {
                return "No";
            }

            if(magazineDict[ransomWord.Key] < ransomWord.Value)
            {
                return "No";
            }
        }

        return  "Yes";
    }

    private Dictionary<string, int> CreateDict(string[] words)
    {
        var dictionary = new Dictionary<string, int>();

        foreach (var word in words)
        {
            if (dictionary.ContainsKey(word))
            {
                dictionary[word] = dictionary[word] + 1;
            }
            else
            {
                dictionary.Add(word, 1);
            }
        }

        return dictionary;
    }
}
```

4. [Stacks: Balanced Brackets](https://www.hackerrank.com/challenges/ctci-balanced-brackets/problem)

```csharp
class Braces
{
    public bool IsRight(string braces)
    {
        var stack = new Stack<char>();

        foreach (var brace in braces.ToCharArray())
        {
            if (brace == '(' || brace == '{' || brace == '[')
            {
                stack.Push(brace);
            }
            else if (brace == ')' || brace == '}' || brace == ']')
            {
                if (stack.Count == 0)
                {
                    return false;
                }
                var lastBrace = stack.Pop();

                if (!IsMatching(lastBrace, brace))
                {
                    return false;
                }
            }
        }

        return stack.Count == 0;
    }

    private bool IsMatching(char lastBrace, char brace)
    {
        return lastBrace == '(' && brace == ')' ||
               lastBrace == '{' && brace == '}' ||
               lastBrace == '[' && brace == ']';
    }
}
```

5. [Queues: A Tale of Two Stacks](https://www.hackerrank.com/challenges/ctci-queue-using-two-stacks/problem)

```csharp
class Step
{
    public int Type { get; }
    public int Value { get; }

    public Step(int type, int value)
    {
        Type = type;
        Value = value;
    }

    public Step(int type)
    {
        Type = type;
    }

    public void Decide(Queue<int> queue)
    {
        switch (Type)
        {
            case 3:
                Console.WriteLine(queue.Peek());
                break;
            case 2:
                queue.Dequeue();
                break;
            case 1:
                queue.Enqueue(Value);
                break;
        }
    }
}

class Solution
{
    static void Main(string[] args)
    {
        Check(int.Parse(Console.ReadLine()));
    }

    static void Check(int inputLength)
    {
        var queue = new Queue<int>();
        for (int i = 0; i < inputLength; i++)
        {
            var step = ReadLine();
            step.Decide(queue);

        }
    }

    static Step ReadLine()
    {
        var line = Console.ReadLine();
        var values = line.Split(' ');

        if (values.Length == 2)
        {
            return new Step(int.Parse(values[0]), int.Parse(values[1]));
        }

        return new Step(int.Parse(values[0]));
    }
}
```

6. [Tries: Contacts](https://www.hackerrank.com/challenges/ctci-contacts/problem)

```csharp
class Solution {
    static void Main(String[] args)
    {
        int n = Convert.ToInt32(Console.ReadLine());
            var contacts = new Dictionary<string, int>();
            for (int a0 = 0; a0 < n; a0++)
            {
                string[] tokens_op = Console.ReadLine().Split(' ');
                string op = tokens_op[0];
                string contact = tokens_op[1];

                if (op == "add")
                {
                    AddContact(contacts, contact);
                }
                else
                {
                    Console.WriteLine(GetContacts(contacts, contact));
                }
            }
    }

    private static int GetContacts(Dictionary<string, int> contacts, string contact)
    {
        contacts.TryGetValue(contact, out var value);

        return value;
    }

    private static void AddContact(Dictionary<string, int> contacts, string contact)
    {
        for (int i = 1; i <= contact.Length; i++)
        {
            var substring = contact.Substring(0, i);

            if (contacts.ContainsKey(substring))
            {
                contacts[substring] = contacts[substring] + 1;
            }
            else
            {
                contacts[substring] = 1;
            }
        }
    }
}
```
