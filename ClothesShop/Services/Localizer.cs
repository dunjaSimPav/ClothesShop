using Microsoft.Extensions.Localization;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace ClothesShop.Services
{
    public class Localizer
    {
        public string Name { get; set; }
        private readonly ResourceManager _rm;
        public Localizer(IStringLocalizerFactory factory, Type T)
        {
            _rm = new ResourceManager(typeof(Resource).FullName, typeof(Resource).Assembly);
        }

        public string this[string key] => GetTranslation(key);

        private string GetTranslation(string key)
        {
            string fallbackTranslation = $"#{key}#";
            try
            {
                string? translation = _rm.GetString(key);
                if (string.IsNullOrEmpty(translation))
                {
                    Debug.WriteLine($"Translation for the key {key} not found, returning {fallbackTranslation}!");
                    return fallbackTranslation;
                }

                return translation;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while trying to get the translation for key {key}, returning {fallbackTranslation}!");
                return fallbackTranslation;
            }
        }
    }
}
