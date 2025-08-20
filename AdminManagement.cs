using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AdminManagement;

[MinimumApiVersion(160)]
public class AdminManagement : BasePlugin
{
    public override string ModuleName => "Gelişmiş Yönetim Menüsü";
    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "NicoV";

    public override void Load(bool hotReload)
    {
        AddCommand("css_admin", "Yönetim menüsünü açar.", OnAdminCommand);
    }

    private void OpenMainMenu(CCSPlayerController player)
    {
        var mainMenu = new ChatMenu($"{ChatColors.LightBlue}Yönetim Menüsü");
        mainMenu.AddMenuOption($"{ChatColors.Green}Oyuncu İşlemleri", (p, option) => ShowPlayerActionsMenu(p));
        mainMenu.AddMenuOption($"{ChatColors.LightBlue}Sunucu İşlemleri", (p, option) => ShowServerActionsMenu(p));
        mainMenu.AddMenuOption($"{ChatColors.Red}Menüyü Kapat", null);
        ChatMenus.OpenMenu(player, mainMenu);
    }

    [RequiresPermissions("@admin/advanced")]
    private void OnAdminCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller == null || !caller.IsValid) return;
        OpenMainMenu(caller);
    }

    private void ShowPlayerActionsMenu(CCSPlayerController caller)
    {
        var playerMenu = new ChatMenu($"{ChatColors.Green}Oyuncu Seç");
        var players = Utilities.GetPlayers().Where(p => p != null && p.IsValid && !p.IsBot).ToList();

        foreach (var p in players)
        {
            playerMenu.AddMenuOption($"{ChatColors.Yellow}{p.PlayerName}", (player, option) => ShowPlayerSubMenu(player, p));
        }

        playerMenu.AddMenuOption($"{ChatColors.Red}Geri Dön", (player, option) => OpenMainMenu(player));
        ChatMenus.OpenMenu(caller, playerMenu);
    }

    private void ShowPlayerSubMenu(CCSPlayerController caller, CCSPlayerController targetPlayer)
    {
        var subMenu = new ChatMenu($"{ChatColors.Red}İşlem: {ChatColors.Yellow}{targetPlayer.PlayerName}");

        subMenu.AddMenuOption($"{ChatColors.Red}Banla (30 dk)", (player, option) => ExecuteBan(targetPlayer));
        subMenu.AddMenuOption($"{ChatColors.Red}Kickle", (player, option) => ExecuteKick(targetPlayer));
        subMenu.AddMenuOption($"{ChatColors.Yellow}Sustur", (player, option) => ExecuteMute(targetPlayer));
        subMenu.AddMenuOption($"{ChatColors.Red}Öldür", (player, option) => ExecuteKill(targetPlayer));

        subMenu.AddMenuOption($"{ChatColors.Red}Geri Dön", (player, option) => ShowPlayerActionsMenu(player));
        ChatMenus.OpenMenu(caller, subMenu);
    }

    private void ShowServerActionsMenu(CCSPlayerController caller)
    {
        var serverMenu = new ChatMenu($"{ChatColors.LightBlue}Sunucu İşlemleri");

        serverMenu.AddMenuOption($"{ChatColors.Red}Roundu Bitir", (player, option) => ExecuteEndRound());
        serverMenu.AddMenuOption($"{ChatColors.Red}Tüm Oyuncuları Öldür", (player, option) => ExecuteKillAll());
        serverMenu.AddMenuOption($"{ChatColors.Green}Küfür Uyarısı", (player, option) => SendSwearWarning());
        serverMenu.AddMenuOption("Hile Uyarısı", (player, option) => SendCheatingWarning());
        serverMenu.AddMenuOption($"{ChatColors.LightBlue}IP Paylaş ve Favorilere Ekle", (player, option) => ShareServerInfo());

        serverMenu.AddMenuOption($"{ChatColors.Red}Geri Dön", (player, option) => OpenMainMenu(player));
        ChatMenus.OpenMenu(caller, serverMenu);
    }

    // Oyuncu İşlemleri
    private void ExecuteBan(CCSPlayerController targetPlayer)
    {
        if (targetPlayer == null || !targetPlayer.IsValid) return;
        Server.ExecuteCommand($"banid {targetPlayer.UserId.Value} 1800 'Yonetici tarafindan banlandi.'");
        Server.PrintToChatAll($" {ChatColors.Red}[ADMIN]{ChatColors.White} {ChatColors.Yellow}{targetPlayer.PlayerName}{ChatColors.White} {ChatColors.Red}30 dakika boyunca yasaklandı.");
    }

    private void ExecuteKick(CCSPlayerController targetPlayer)
    {
        if (targetPlayer == null || !targetPlayer.IsValid) return;
        Server.ExecuteCommand($"kickid {targetPlayer.UserId.Value}");
        Server.PrintToChatAll($" {ChatColors.Red}[ADMIN]{ChatColors.White} {ChatColors.Yellow}{targetPlayer.PlayerName}{ChatColors.White} {ChatColors.Red}sunucudan atıldı.");
    }

    private void ExecuteMute(CCSPlayerController targetPlayer)
    {
        if (targetPlayer == null || !targetPlayer.IsValid) return;
        targetPlayer.VoiceFlags = VoiceFlags.Muted;
        Server.PrintToChatAll($" {ChatColors.Red}[ADMIN]{ChatColors.White} {ChatColors.Yellow}{targetPlayer.PlayerName}{ChatColors.White} {ChatColors.Red}susturuldu.");
    }

    private void ExecuteKill(CCSPlayerController targetPlayer)
    {
        if (targetPlayer == null || !targetPlayer.IsValid) return;
        // Düzeltme: `CommitSuicide` artık parametre istiyor.
        targetPlayer.Pawn.Value.CommitSuicide(false, false);
        Server.PrintToChatAll($" {ChatColors.Red}[ADMIN]{ChatColors.White} {ChatColors.Yellow}{targetPlayer.PlayerName}{ChatColors.White} {ChatColors.Red}öldürüldü.");
    }

    // Sunucu İşlemleri
    private void ExecuteEndRound()
    {
        Server.ExecuteCommand("mp_end_round");
        Server.PrintToChatAll($" {ChatColors.Red}[ADMIN]{ChatColors.White} {ChatColors.Red}Round sonlandırıldı.");
    }

    private void ExecuteKillAll()
    {
        foreach (var player in Utilities.GetPlayers().Where(p => p != null && p.IsValid && !p.IsBot))
        {
            // Düzeltme: `CommitSuicide` artık parametre istiyor.
            player.Pawn.Value.CommitSuicide(false, false);
        }
        Server.PrintToChatAll($" {ChatColors.Red}[ADMIN]{ChatColors.White} {ChatColors.Red}Tüm oyuncular öldürüldü.");
    }

    private void SendSwearWarning()
    {
        Server.PrintToChatAll($" {ChatColors.Red}[UYARI]{ChatColors.White} {ChatColors.Red}Lütfen sohbet kurallarına uyunuz. Küfür etmek yasaktır!");
    }

    private void SendCheatingWarning()
    {
        Server.PrintToChatAll($" {ChatColors.Red}[UYARI]{ChatColors.White} {ChatColors.Red}Hile kullanımı yasaktır. Hile kullanan oyuncular kalıcı olarak banlanacaktır!");
    }

    private void ShareServerInfo()
    {
        string ip = "185.193.165.27";
        string port = "27015";
        Server.PrintToChatAll($" {ChatColors.Red}[SUNUCU BİLGİSİ]{ChatColors.White} {ChatColors.LightBlue}Sunucuyu favorilerinize ekleyin!");
        Server.PrintToChatAll($" {ChatColors.LightBlue}IP Adresi:{ChatColors.White} {ip}:{port}");
    }

}
