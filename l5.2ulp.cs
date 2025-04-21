артем, [21.04.2025 17:35]
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public interface IProtocol
{
    string Protocol();
}

[XmlInclude(typeof(Email))]
[XmlInclude(typeof(SMS))]
[XmlInclude(typeof(Letter))]
[XmlInclude(typeof(ElectronicMessage))]
public abstract class Message : IProtocol
{
    public abstract string Protocol();
    public abstract void PrintDetails();
}

[XmlRoot("MessageStorage")]
public class MessageStorage
{
    [XmlElement("Message")]
    public List<Message> Messages { get; set; } = new List<Message>();
}

public class Program
{
    static void Main()
    {
        string filePath = "messages.xml";
        InitializeFile(filePath);

        AddMessage(filePath, new Email("bobr@mail.ru", "crocodil@mail.ru", "Hello!"));
        AddMessage(filePath, new SMS("+7900-800-10-10", "Hi!"));
        AddMessage(filePath, new Letter("Staropupunski 12", "Pushkina 120", "This is a letter."));
        AddMessage(filePath, new ElectronicMessage("This is an electronic message."));

        FindMessage(filePath, "Hello!");
        RemoveMessage(filePath, "Hi!");
        FindMessage(filePath, "Hi!");

        Console.WriteLine($"Файл сохранен по пути: {Path.GetFullPath(filePath)}");
    }

    static void InitializeFile(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка инициализации: {ex.Message}");
        }
    }

    static void AddMessage(string filePath, Message message)
    {
        try
        {
            var storage = LoadMessages(filePath);
            storage.Messages.Add(message);
            SaveMessages(filePath, storage);
            Console.WriteLine($"Добавлено сообщение типа {message.GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка добавления: {ex.Message}");
        }
    }

    static void RemoveMessage(string filePath, string content)
    {
        try
        {
            var storage = LoadMessages(filePath);
            int removed = storage.Messages.RemoveAll(m =>
                (m is ElectronicMessage em && em.Content == content) ||
                (m is SMS sms && sms.Text == content));

            if (removed > 0)
            {
                SaveMessages(filePath, storage);
                Console.WriteLine($"Сообщение '{content}' удалено");
            }
            else
            {
                Console.WriteLine("Сообщение не найдено");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка удаления: {ex.Message}");
        }
    }

    static void FindMessage(string filePath, string searchText)
    {
        try
        {
            var storage = LoadMessages(filePath);
            var found = storage.Messages.Find(m =>
                m switch
                {
                    Email e => e.Subject?.Contains(searchText) ?? false,
                    SMS s => s.Text?.Contains(searchText) ?? false,
                    Letter l => l.Content?.Contains(searchText) ?? false,
                    ElectronicMessage em => em.Content?.Contains(searchText) ?? false,
                    _ => false
                });

артем, [21.04.2025 17:35]
if (found != null)
            {
                Console.WriteLine("Найдено сообщение:");
                found.PrintDetails();
                Console.WriteLine($"Протокол: {found.Protocol()}\n");
            }
            else
            {
                Console.WriteLine($"Сообщение '{searchText}' не найдено\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка поиска: {ex.Message}");
        }
    }static MessageStorage LoadMessages(string filePath)
    {
        if (!File.Exists(filePath)) return new MessageStorage();

        try
        {
            var serializer = new XmlSerializer(typeof(MessageStorage));
            using var reader = new StreamReader(filePath);
            return (MessageStorage)serializer.Deserialize(reader) ?? new MessageStorage();
        }
        catch
        {
            return new MessageStorage();
        }
    }

    static void SaveMessages(string filePath, MessageStorage storage)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(MessageStorage));
            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, storage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения: {ex.Message}");
        }
    }
}

public class Email : Message
{
    public string Sender { get; set; }
    public string Recipient { get; set; }
    public string Subject { get; set; }

    // Конструктор для XML-сериализации
    public Email() { }

    public Email(string sender, string recipient, string subject)
    {
        Sender = sender;
        Recipient = recipient;
        Subject = subject;
    }

    public override string Protocol() => "Email Protocol";

    public override void PrintDetails()
    {
        Console.WriteLine($"От: {Sender}\nКому: {Recipient}\nТема: {Subject}");
    }
}

public class SMS : Message
{
    public string PhoneNumber { get; set; }
    public string Text { get; set; }

    // Конструктор для XML-сериализации
    public SMS() { }

    public SMS(string phoneNumber, string text)
    {
        PhoneNumber = phoneNumber;
        Text = text;
    }

    public override string Protocol() => "SMS Protocol";

    public override void PrintDetails()
    {
        Console.WriteLine($"Номер: {PhoneNumber}\nТекст: {Text}");
    }
}

public class Letter : Message
{
    public string SenderAddress { get; set; }
    public string RecipientAddress { get; set; }
    public string Content { get; set; }

    // Конструктор для XML-сериализации
    public Letter() { }

    public Letter(string senderAddress, string recipientAddress, string content)
    {
        SenderAddress = senderAddress;
        RecipientAddress = recipientAddress;
        Content = content;
    }

    public override string Protocol() => "Letter Protocol";

    public override void PrintDetails()
    {
        Console.WriteLine($"Адрес отправителя: {SenderAddress}\nАдрес получателя: {RecipientAddress}\nСодержание: {Content}");
    }
}

public class ElectronicMessage : Message
{
    public string Content { get; set; }

    // Конструктор для XML-сериализации
    public ElectronicMessage() { }

    public ElectronicMessage(string content)
    {
        Content = content;
    }

    public override string Protocol() => "Electronic Message Protocol";

    public override void PrintDetails()
    {
        Console.WriteLine($"Содержание: {Content}");
    }
}
