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
                // Dateiinhalt lesen
                string[] lines = File.ReadAllLines(filePath);

                // Erste Zeile entfernen
                if (lines.Length > 0)
                {
                    lines = lines[1..]; // Array-Slice: Alles außer der ersten Zeile
                }

                // Bearbeitung jeder Zeile
                bool deleteRest = false; // Markierung, wenn Inhalte nach der letzten Zahl gelöscht werden sollen
                for (int i = 0; i < lines.Length; i++)
                {
                    if (deleteRest)
                    {
                        lines[i] = ""; // Lösche alle Inhalte nach der Markierung
                        continue;
                    }

                    // Prüfen, ob nach der letzten Zahl ein Wort kommt
                    if (Regex.IsMatch(lines[i], @"\d+\s+[A-Za-z]"))
                    {
                        deleteRest = true; // Markiere, dass ab dieser Zeile gelöscht werden soll
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


                // **Zusätzliche Prüfung der letzten Zeile**
                string[] finalLines = content.Split(Environment.NewLine);
                if (finalLines.Length > 0 && !Regex.IsMatch(finalLines[^1], @"^\d+([.,]\d+)*(\s+\d+([.,]\d+)*)*$"))
                {
                    // Letzte Zeile entfernen, wenn sie keine reinen Zahlen enthält
                    finalLines = finalLines[..^1];
                }

                // Erneutes Zusammenfügen nach Überprüfung
                content = string.Join(Environment.NewLine, finalLines);


                // Punkte durch Kommas ersetzen
                content = content.Replace(".", ",");

                // Neuer Dateiname erstellen
                string directory = Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string newFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_formatted{extension}");

                // Neue Datei speichern
                File.WriteAllText(newFilePath, content);

                Console.WriteLine($"Die Datei wurde erfolgreich bearbeitet und unter '{newFilePath}' gespeichert.");
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
}