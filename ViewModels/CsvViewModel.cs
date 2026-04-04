using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using WeaponCollection.Models;
using WeaponCollection.Services;

namespace WeaponCollection.ViewModels;

public class ColumnSelection : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _name = "";

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class CsvViewModel : ViewModelBase
{
    public IRelayCommand BackCommand { get; }
    public IAsyncRelayCommand ExportCsvCommand { get; }
    public IAsyncRelayCommand ImportCsvCommand { get; }

    private readonly CsvService _csvService = new();

    // Price ajouté dans les colonnes disponibles
    public List<ColumnSelection> AvailableColumns { get; } = new()
    {
        new ColumnSelection { Name = "Name", IsSelected = true },
        new ColumnSelection { Name = "Type", IsSelected = true },
        new ColumnSelection { Name = "Damage", IsSelected = true },
        new ColumnSelection { Name = "Weight", IsSelected = true },
        new ColumnSelection { Name = "Price", IsSelected = true },
        new ColumnSelection { Name = "Era", IsSelected = false },
        new ColumnSelection { Name = "Country", IsSelected = false },
        new ColumnSelection { Name = "Description", IsSelected = false },
        new ColumnSelection { Name = "PictureUrl", IsSelected = false }
    };

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _showSuccess;
    public bool ShowSuccess
    {
        get => _showSuccess;
        set => SetProperty(ref _showSuccess, value);
    }

    public CsvViewModel(IRelayCommand backCommand)
    {
        BackCommand = backCommand;
        ExportCsvCommand = new AsyncRelayCommand(ExportCsv);
        ImportCsvCommand = new AsyncRelayCommand(ImportCsv);
    }

    private async Task ExportCsv()
    {
        var window = (Avalonia.Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (window == null) return;

        var selectedColumns = AvailableColumns
            .Where(c => c.IsSelected)
            .Select(c => c.Name)
            .ToList();

        if (selectedColumns.Count == 0)
        {
            StatusMessage = "⚠ Sélectionnez au moins une colonne !";
            ShowSuccess = true;
            await Task.Delay(3000);
            ShowSuccess = false;
            return;
        }

        await _csvService.ExportAsync(
            new List<Weapon>(MyGlobals.MyWeapons),
            selectedColumns,
            window);

        StatusMessage = $"✅ Export réussi ! ({selectedColumns.Count} colonnes exportées)";
        ShowSuccess = true;
        await Task.Delay(3000);
        ShowSuccess = false;
    }

    private async Task ImportCsv()
    {
        var window = (Avalonia.Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (window == null) return;

        var weapons = await _csvService.ImportAsync(window);

        foreach (var weapon in weapons)
            MyGlobals.MyWeapons.Add(weapon);

        // Sauvegarde sur le serveur après import
        var service = new WeaponService();
        await service.SaveWeaponsAsync(new List<Weapon>(MyGlobals.MyWeapons));

        StatusMessage = $"✅ {weapons.Count} armes importées et sauvegardées !";
        ShowSuccess = true;
        await Task.Delay(3000);
        ShowSuccess = false;
    }
}
