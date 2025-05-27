using Blazored.LocalStorage;
using MudBlazor;
using MudBlazor.Utilities;

namespace ManagementPortal.Client.Themes
{
    /// <summary>
    /// Componente para manejar el dark mode en la aplicación.
    /// </summary>
    public class CustomThemeProvider
    {
        private readonly ILocalStorageService _localStorage;
        private bool _isDarkMode;

        private MudTheme _customTheme = new()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = new MudColor("#212121"),
                Secondary = new MudColor("#ECEFF1"),
                Tertiary = new MudColor("#FAFAFA"),
                TextPrimary = new MudColor("#37474F"),
                TextSecondary = new MudColor("#546E7A"),
                Error = new MudColor("#F44336"),
                Info = new MudColor("#5c9fa1"),
                Warning = new MudColor("#FFA726"),
                Black = new MudColor("#212121"),
            },
            PaletteDark = new PaletteDark()
            {
                Primary = new MudColor("#5c9fa1"),
                Secondary = new MudColor("#455A64"),
                Tertiary = new MudColor("#212121"),
                TextPrimary = new MudColor("#CFD8DC"),
                TextSecondary = new MudColor("#B0BEC5"),
                Error = new MudColor("#F44336"),
                Info = new MudColor("#5c9fa1"),
                Warning = new MudColor("#FFA726"),
            },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomThemeProvider"/> class.
        /// Inicializa una nueva instancia de la clase <see cref="CustomThemeProvider"/>.
        /// </summary>
        /// <param name="localStorage">Interfaz para menejar el local storage.</param>
        public CustomThemeProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        /// <summary>
        /// Atributo para determinar dark mode activo.
        /// </summary>
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    SaveThemePreferenceAsync(_isDarkMode).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Tema para el dark mode light y dark.
        /// </summary>
        public MudTheme CustomTheme
        {
            get => _customTheme;
            set => _customTheme = value;
        }

        /// <summary>
        /// Inicializa el componente para recuperar el estado de local storage.
        /// </summary>
        /// <returns>No retorna valor ya que se implementa como un task.</returns>
        public async Task InitializeAsync()
        {
            var isDark = await _localStorage.GetItemAsync<bool>("isDarkMode");
            IsDarkMode = isDark; // Establece el modo basado en el valor recuperado
        }

        /// <summary>
        /// Guarda preferencia en local storage.
        /// </summary>
        /// <param name="isDarkMode">Booleano para indicar dark mode activo.</param>
        /// <returns>No retorna valor ya que se implementa como Task.</returns>
        private async Task SaveThemePreferenceAsync(bool isDarkMode)
        {
            await _localStorage.SetItemAsync("isDarkMode", isDarkMode);
        }
    }
}