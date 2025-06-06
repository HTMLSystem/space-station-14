using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Server)]
public sealed class PersistenceSave : LocalizedEntityCommands
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IEntitySystemManager _system = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    public override string Command => "persistencesave";
    public override string Description => "Saves server data to a persistence file to be loaded later.";
    public override string Help => "persistencesave [mapId] [filePath - default: game.map (CCVar) ]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1 || args.Length > 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!int.TryParse(args[0], out var intMapId))
        {
            shell.WriteError(Loc.GetString("cmd-parse-failure-integer", ("arg", args[0])));
            return;
        }

        var mapId = new MapId(intMapId);
        if (!_map.MapExists(mapId))
        {
            shell.WriteError(Loc.GetString("cmd-savemap-not-exist"));
            return;
        }

        var saveFilePath = (args.Length > 1 ? args[1] : null) ?? _config.GetCVar(CCVars.GameMap);
        if (string.IsNullOrWhiteSpace(saveFilePath))
        {
            shell.WriteError(Loc.GetString("cmd-persistencesave-no-path", ("cvar", nameof(CCVars.GameMap))));
            return;
        }

        var mapLoader = _system.GetEntitySystem<MapLoaderSystem>();
        mapLoader.TrySaveMap(mapId, new ResPath(saveFilePath));
        shell.WriteLine(Loc.GetString("cmd-savemap-success"));
    }
}
