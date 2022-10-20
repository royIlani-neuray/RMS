
namespace WebService.Database;

public class Database
{
    public static void DatabaseInit()
    {
        // create storage folders for entities

        if (!System.IO.Directory.Exists(DeviceStorage.StoragePath))
        {
            System.Console.WriteLine("Creating devices storage folder.");
            System.IO.Directory.CreateDirectory(DeviceStorage.StoragePath);
        }

        if (!System.IO.Directory.Exists(TemplateStorage.StoragePath))
        {
            System.Console.WriteLine("Creating templates storage folder.");
            System.IO.Directory.CreateDirectory(TemplateStorage.StoragePath);
        }
    }
}