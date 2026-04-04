using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MongoDB.Bson;
using WeaponCollection.Models;

namespace WeaponCollection.Services;

public class WeaponService
{
    private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });

    private const string BaseUrl = "http://185.157.245.38:8080/json";
    private const string FileName = "WeaponCollection.json";

    public async Task<List<Weapon>> LoadWeaponsAsync()
    {
        try
        {
            var url = $"{BaseUrl}?FileName={FileName}";
            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<Weapon>();

            await using var contentStream = await response.Content.ReadAsStreamAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var dtos = await JsonSerializer.DeserializeAsync<List<WeaponDto>>(contentStream, options);
            if (dtos == null) return new List<Weapon>();

            var weapons = new List<Weapon>();
            foreach (var dto in dtos)
            {
                Bitmap? picture = null;
                if (!string.IsNullOrWhiteSpace(dto.PictureUrl))
                {
                    try
                    {
                        if (File.Exists(dto.PictureUrl))
                        {
                            picture = new Bitmap(dto.PictureUrl);
                        }
                        else
                        {
                            var imageBytes = await _httpClient.GetByteArrayAsync(dto.PictureUrl);
                            using var ms = new MemoryStream(imageBytes);
                            picture = new Bitmap(ms);
                        }
                    }
                    catch
                    {
                        picture = null;
                    }
                }

                weapons.Add(new Weapon
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = dto.Name ?? "",
                    Type = dto.Type ?? "",
                    Damage = dto.Damage,
                    Weight = dto.Weight,
                    Price = dto.Price, // ✅ Price ajouté
                    Description = dto.Description ?? "",
                    Era = dto.Era ?? "",
                    Country = dto.Country ?? "",
                    PictureUrl = dto.PictureUrl ?? "",
                    Picture = picture
                });
            }
            return weapons;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement JSON : {ex.Message}");
            return new List<Weapon>();
        }
    }

    public async Task SaveWeaponsAsync(List<Weapon> weapons)
    {
        try
        {
            var dtos = new List<WeaponDto>();
            foreach (var w in weapons)
            {
                dtos.Add(new WeaponDto
                {
                    Name = w.Name,
                    Type = w.Type,
                    Damage = w.Damage,
                    Weight = w.Weight,
                    Price = w.Price, // ✅ Price ajouté
                    Description = w.Description,
                    Era = w.Era,
                    Country = w.Country,
                    PictureUrl = w.PictureUrl
                });
            }

            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, dtos);
            memoryStream.Position = 0;

            var fileContent = new StreamContent(memoryStream)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            var content = new MultipartFormDataContent
            {
                { fileContent, "file", FileName }
            };

            using var response = await _httpClient.PostAsync(BaseUrl, content);
            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Erreur sauvegarde : {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur sauvegarde JSON : {ex.Message}");
        }
    }

    private class WeaponDto
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Damage { get; set; }
        public double Weight { get; set; }
        public double Price { get; set; } // ✅ Price ajouté
        public string? Description { get; set; }
        public string? Era { get; set; }
        public string? Country { get; set; }
        public string? PictureUrl { get; set; }
    }
}
