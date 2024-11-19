using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Gib den Pfad zur Datei ein:");
        string filePath = Console.ReadLine();

        if (filePath != null)
        {
            filePath = filePath.Replace("\"", "");
        }

        if (File.Exists(filePath))
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string formattedFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_formatted{extension}");

                // FileSystemWatcher einrichten
                FileSystemWatcher watcher = new FileSystemWatcher(directory, Path.GetFileName(filePath));
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;

                watcher.Changed += (sender, e) =>
                {
                    Console.WriteLine($"Änderung erkannt: {e.FullPath}");
                    FormatFile(filePath, formattedFilePath);
                };

                FormatFile(filePath, formattedFilePath);

                // Watcher starten
                watcher.EnableRaisingEvents = true;
                Console.WriteLine($"Überwachung der Datei '{filePath}' gestartet. Drücke Enter, um zu beenden.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Es ist ein Fehler aufgetreten: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Die angegebene Datei existiert nicht.");
        }
    }

    static void FormatFile(string inputFilePath, string outputFilePath)
    {
        try
        {
            // Dateiinhalt lesen
            string[] lines = File.ReadAllLines(inputFilePath);

            // Erste Zeile entfernen
            if (lines.Length > 0)
            {
                lines = lines[1..]; // Array-Slice: Alles außer der ersten Zeile
            }

            // Bearbeitung jeder Zeile
            bool deleteRest = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (deleteRest)
                {
                    lines[i] = ""; // Lösche alle Inhalte nach der Markierung
                    continue;
                }

                if (Regex.IsMatch(lines[i], @"\d+\s+[A-Za-z]"))
                {
                    deleteRest = true;
                    lines[i] = Regex.Replace(lines[i], @"\s+[A-Za-z].*", ""); // Entferne alles nach der letzten Zahl
                }

                // Semikolon vor jedem Whitespace hinzufügen
                lines[i] = Regex.Replace(lines[i], @"(\S)\s(?![\r\n])", "$1; ");

                // Whitespace und Semikolon am Zeilenende entfernen
                lines[i] = Regex.Replace(lines[i], @"[;\s]+$", "");

                // Whitespace außer Zeilenumbrüche entfernen
                lines[i] = Regex.Replace(lines[i], @"[^\S\r\n]+", "");
            }

            // Zusammenfügen der Zeilen
            string content = string.Join(Environment.NewLine, lines).Trim();

            // Zusätzliche Prüfung der letzten Zeile
            string[] finalLines = content.Split(Environment.NewLine);
            if (finalLines.Length > 0 && !Regex.IsMatch(finalLines[^1], @"^\d+([.,]\d+)*(\s+\d+([.,]\d+)*)*$"))
            {
                finalLines = finalLines[..^1];
            }

            content = string.Join(Environment.NewLine, finalLines);

            // Punkte durch Kommas ersetzen
            content = content.Replace(".", ",");

            // Neue Datei speichern
            File.WriteAllText(outputFilePath, content);
            Console.WriteLine($"Die formatierte Datei wurde aktualisiert: '{outputFilePath}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Formatieren der Datei: {ex.Message}");
        }
    }
}