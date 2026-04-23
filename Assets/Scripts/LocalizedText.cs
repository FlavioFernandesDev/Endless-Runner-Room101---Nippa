using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class LocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;

    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        SettingsManager.LoadAndApply();
        SettingsManager.LanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        SettingsManager.LanguageChanged -= Refresh;
    }

    public void AssignKey(string key)
    {
        localizationKey = key;
        Refresh();
    }

    public void Refresh()
    {
        if (_text == null)
        {
            _text = GetComponent<TMP_Text>();
        }

        if (_text == null || string.IsNullOrWhiteSpace(localizationKey))
        {
            return;
        }

        _text.text = LocalizationTable.Get(localizationKey, SettingsManager.Language);
    }
}

internal static class LocalizationTable
{
    private static readonly Dictionary<string, (string english, string portuguese)> Entries = new()
    {
        ["menu.start"] = ("Start Game", "Iniciar Jogo"),
        ["menu.quit"] = ("Quit", "Sair"),
        ["menu.settings"] = ("Settings", "Definicoes"),
        ["menu.game_over_prompt"] = ("GAME OVER!\n\nCLICK TO PLAY AGAIN!\n", "FIM DE JOGO!\n\nCLICA PARA JOGAR OUTRA VEZ!\n"),
        ["settings.title"] = ("Settings", "Definicoes"),
        ["settings.volume"] = ("Volume", "Volume"),
        ["settings.language"] = ("Language", "Idioma"),
        ["settings.fullscreen"] = ("Fullscreen", "Ecra Inteiro"),
        ["settings.quality"] = ("Quality", "Qualidade"),
        ["settings.close"] = ("Close", "Fechar"),
        ["stage.play"] = ("PLAY", "JOGAR"),
        ["stage.room_run"] = ("Room Run", "Corrida do Quarto"),
        ["stage.quit"] = ("QUIT", "SAIR"),
        ["info.jump"] = ("JUMP", "SALTAR"),
        ["info.move_left"] = ("MOVE LEFT", "MOVER A ESQUERDA"),
        ["info.move_right"] = ("MOVE RIGHT", "MOVER A DIREITA"),
        ["info.spacebar"] = ("SPACEBAR", "BARRA DE ESPACO"),
        ["info.loading"] = ("LOADING...", "A CARREGAR...")
    };

    public static string Get(string key, AppLanguage language)
    {
        if (!Entries.TryGetValue(key, out (string english, string portuguese) entry))
        {
            return key;
        }

        return language == AppLanguage.PT ? entry.portuguese : entry.english;
    }

    public static bool TryGetKeyForValue(string value, out string key)
    {
        foreach (KeyValuePair<string, (string english, string portuguese)> entry in Entries)
        {
            if (entry.Value.english == value || entry.Value.portuguese == value)
            {
                key = entry.Key;
                return true;
            }
        }

        key = null;
        return false;
    }
}
