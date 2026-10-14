using DownloadsSorter_DoSo;

Console.WriteLine("=== Проверка FileRoutingService ===");

var router = new FileRoutingService();

string[] testFiles = ["report.pdf", "photo.PNG", "archive.zip", "setup.exe", "unknown.xyz", "no_ext"];

foreach (var fileName in testFiles)
{
    var extension = Path.GetExtension(fileName);
    var targetFolder = router.GetDestinationFolder(extension);
    
    Console.WriteLine($"Файл: {fileName,-15} -> Папка: {targetFolder}");
}