using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustMeetinPoint.Maui.Features.Groups.Models;
using JustMeetinPoint.Maui.Features.Groups.Services;
using System.Collections.ObjectModel;

namespace JustMeetinPoint.Maui.Features.Groups.ViewModels;

public partial class CreateGroupViewModel : ObservableObject
{
    private readonly IGroupService _groupService;

    public CreateGroupViewModel(IGroupService groupService)
    {
        _groupService = groupService;

        Categories = new ObservableCollection<CategoryOptionModel>
        {
            new() { Name = "Comer" },
            new() { Name = "Café" },
            new() { Name = "Ocio" },
            new() { Name = "Trabajo" },
            new() { Name = "Deporte" }
        };

        Methods = new ObservableCollection<MethodOptionModel>
        {
            new()
            {
                Name = "Centroide",
                Description = "Método inicial disponible actualmente.",
                Value = "centroid",
                IsSelected = true,
                IsEnabled = true
            },
            new()
            {
                Name = "Óptimo",
                Description = "Próximamente.",
                Value = "optimal",
                IsSelected = false,
                IsEnabled = false
            },
            new()
            {
                Name = "Por recomendación",
                Description = "Próximamente.",
                Value = "recommended",
                IsSelected = false,
                IsEnabled = false
            }
        };

        SelectedMethod = "centroid";
    }

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string selectedMethod = "centroid";

    [ObservableProperty]
    private string selectedCategory = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ObservableCollection<CategoryOptionModel> Categories { get; }

    public ObservableCollection<MethodOptionModel> Methods { get; }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    [RelayCommand]
    private void SelectCategory(CategoryOptionModel option)
    {
        if (option is null)
            return;

        foreach (var item in Categories)
            item.IsSelected = false;

        option.IsSelected = true;
        SelectedCategory = option.Name;
    }

    [RelayCommand]
    private void SelectMethod(MethodOptionModel option)
    {
        if (option is null || !option.IsEnabled)
            return;

        foreach (var item in Methods)
            item.IsSelected = false;

        option.IsSelected = true;
        SelectedMethod = option.Value;
    }

    [RelayCommand]
    private async Task CreateGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Introduce un nombre.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedCategory))
        {
            ErrorMessage = "Selecciona un motivo.";
            return;
        }

        try
        {
            ErrorMessage = string.Empty;

            var result = await _groupService.CreateGroupAsync(
                Name.Trim(),
                Description?.Trim() ?? string.Empty,
                SelectedMethod,
                SelectedCategory);

            if (result is null || string.IsNullOrWhiteSpace(result.GroupCode))
            {
                ErrorMessage = "No se pudo crear el grupo.";
                return;
            }

            await Shell.Current.GoToAsync(
                $"group-lobby?groupCode={Uri.EscapeDataString(result.GroupCode)}&isCurrentUserHost=true");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al crear el grupo: {ex.Message}";
        }
    }
}