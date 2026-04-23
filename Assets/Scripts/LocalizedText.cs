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
        ["menu.achievements"] = ("Achievements", "Conquistas"),
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
        ["ach.page.title"] = ("Achievements", "Conquistas"),
        ["ach.page.subtitle"] = ("Push further every run and build your collection of milestones.", "Vai mais longe a cada run e desbloqueia a tua colecao de marcos."),
        ["ach.page.back"] = ("Back", "Voltar"),
        ["ach.status.unlocked"] = ("UNLOCKED", "DESBLOQUEADA"),
        ["ach.status.locked"] = ("IN PROGRESS", "EM PROGRESSO"),
        ["ach.summary.unlocked"] = ("Unlocked", "Desbloqueadas"),
        ["ach.summary.unlocked_value"] = ("{0}/{1}", "{0}/{1}"),
        ["ach.summary.best_run"] = ("Best Run", "Melhor Run"),
        ["ach.summary.coins"] = ("Total Coins", "Total de Moedas"),
        ["ach.summary.keys"] = ("Total Keys", "Total de Chaves"),
        ["ach.category.progress"] = ("Progress", "Progresso"),
        ["ach.category.distance"] = ("Distance", "Distancia"),
        ["ach.category.coins"] = ("Coins", "Moedas"),
        ["ach.category.keys"] = ("Keys", "Chaves"),
        ["ach.progress.label"] = ("Progress", "Progresso"),
        ["ach.progress.distance"] = ("{0} / {1}m", "{0} / {1}m"),
        ["ach.progress.coins"] = ("{0} / {1} coins", "{0} / {1} moedas"),
        ["ach.progress.keys"] = ("{0} / {1} keys", "{0} / {1} chaves"),
        ["ach.progress.value"] = ("{0} / {1}", "{0} / {1}"),
        ["ach.distance.100.title"] = ("First Sprint", "Primeiro Sprint"),
        ["ach.distance.100.desc"] = ("Run 100 meters in a single run.", "Corre 100 metros numa unica run."),
        ["ach.distance.250.title"] = ("Hallway Hustle", "Corredor Imparavel"),
        ["ach.distance.250.desc"] = ("Run 250 meters in a single run.", "Corre 250 metros numa unica run."),
        ["ach.distance.500.title"] = ("Room Raider", "Invasor de Quartos"),
        ["ach.distance.500.desc"] = ("Run 500 meters in a single run.", "Corre 500 metros numa unica run."),
        ["ach.distance.1000.title"] = ("Endless Guest", "Hospede Inesgotavel"),
        ["ach.distance.1000.desc"] = ("Run 1000 meters in a single run.", "Corre 1000 metros numa unica run."),
        ["ach.coins.25.title"] = ("Pocket Change", "Trocos no Bolso"),
        ["ach.coins.25.desc"] = ("Collect 25 coins in total.", "Recolhe 25 moedas no total."),
        ["ach.coins.100.title"] = ("Golden Habit", "Habito Dourado"),
        ["ach.coins.100.desc"] = ("Collect 100 coins in total.", "Recolhe 100 moedas no total."),
        ["ach.coins.250.title"] = ("Vault Fever", "Febre do Cofre"),
        ["ach.coins.250.desc"] = ("Collect 250 coins in total.", "Recolhe 250 moedas no total."),
        ["ach.keys.3.title"] = ("Key Collector", "Colecionador de Chaves"),
        ["ach.keys.3.desc"] = ("Collect 3 keys in total.", "Recolhe 3 chaves no total."),
        ["ach.keys.10.title"] = ("Master Keychain", "Mestre do Porta-Chaves"),
        ["ach.keys.10.desc"] = ("Collect 10 keys in total.", "Recolhe 10 chaves no total."),
        ["ach.keys.25.title"] = ("Hotel Locksmith", "Chaveiro do Hotel"),
        ["ach.keys.25.desc"] = ("Collect 25 keys in total.", "Recolhe 25 chaves no total."),
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
