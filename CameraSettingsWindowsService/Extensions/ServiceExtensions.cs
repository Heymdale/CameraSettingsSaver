using CameraSettingsSaver.Resources;

namespace CameraSettingsWindowsService.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddServiceLocalization(this IServiceCollection services, Language language)
        {
            services.AddSingleton(sp =>
            {
                var localization = new Localization();
                localization.SetLanguage(language);
                return localization;
            });
        }
    }
}