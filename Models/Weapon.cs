using Avalonia.Media;
using MongoDB.Bson;

namespace WeaponCollection.Models;

public class Weapon
{
    public Weapon() { }

    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Damage { get; set; }
    public double Weight { get; set; }
    public double Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Era { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    internal IImage? Picture { get; set; }
}
