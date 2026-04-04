using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using WeaponCollection.Models;

namespace WeaponCollection.Services;

public class CsvService
{
    private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });

    public async Task ExportAsync(List<Weapon> weapons, List<string> selectedColumns, Window window)
    {
        try
        {
            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Exporter en CSV",
                SuggestedFileName = "weapons.csv",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } }
                }
            });

            if (file == null) return;

            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);

            await writer.WriteLineAsync(string.Join(",", selectedColumns.Select(c => $"\"{c}\"")));

            foreach (var weapon in weapons)
            {
                var values = new List<string>();
                foreach (var col in selectedColumns)
                {
                    var value = col switch
                    {
                        "Name" => weapon.Name,
                        "Type" => weapon.Type,
                        "Damage" => weapon.Damage.ToString(CultureInfo.InvariantCulture),
                        "Weight" => weapon.Weight.ToString(CultureInfo.InvariantCulture),
                        "Price" => weapon.Price.ToString(CultureInfo.InvariantCulture),
                        "Era" => weapon.Era,
                        "Country" => weapon.Country,
                        "Description" => weapon.Description,
                        "PictureUrl" => weapon.PictureUrl,
                        _ => ""
                    };
                    values.Add($"\"{value}\"");
                }
                await writer.WriteLineAsync(string.Join(",", values));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur export CSV : {ex.Message}");
        }
    }

    public async Task<List<Weapon>> ImportAsync(Window window)
    {
        var weapons = new List<Weapon>();
        try
        {
            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Importer un CSV",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("CSV") { Patterns = new[] { "*.csv" } }
                }
            });

            if (files.Count == 0) return weapons;

            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream);

            var header = await reader.ReadLineAsync();
            if (header == null) return weapons;

            var columns = header.Split(',')
                .Select(c => c.Trim().Trim('"'))
                .ToList();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',')
                    .Select(v => v.Trim().Trim('"'))
                    .ToList();

                var weapon = new Weapon();

                for (int i = 0; i < columns.Count && i < values.Count; i++)
                {
                    switch (columns[i])
                    {
                        case "Name": weapon.Name = values[i]; break;
                        case "Type": weapon.Type = values[i]; break;
                        case "Damage":
                            int.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var dmg);
                            weapon.Damage = dmg;
                            break;
                        case "Weight":
                            double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var wgt);
                            weapon.Weight = wgt;
                            break;
                        case "Price":
                            double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var prc);
                            weapon.Price = prc;
                            break;
                        case "Era": weapon.Era = values[i]; break;
                        case "Country": weapon.Country = values[i]; break;
                        case "Description": weapon.Description = values[i]; break;
                        case "PictureUrl": weapon.PictureUrl = values[i]; break;
                    }
                }

                // Charge l'image depuis l'URL
                if (!string.IsNullOrWhiteSpace(weapon.PictureUrl))
                {
                    try
                    {
                        if (File.Exists(weapon.PictureUrl))
                        {
                            weapon.Picture = new Bitmap(weapon.PictureUrl);
                        }
                        else
                        {
                            var imageBytes = await _httpClient.GetByteArrayAsync(weapon.PictureUrl);
                            using var ms = new MemoryStream(imageBytes);
                            weapon.Picture = new Bitmap(ms);
                        }
                    }
                    catch
                    {
                        weapon.Picture = null;
                    }
                }

                weapons.Add(weapon);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur import CSV : {ex.Message}");
        }
        return weapons;
    }
}
