using OpenConstructionSet.Data;
using OpenConstructionSet.Installations;
using OpenConstructionSet.Mods;
using OpenConstructionSet.Mods.Context;

const string ModName = "OCSP SCAR's pathfinding fix";
const string ModFileName = ModName + ".mod";
const string ReferenceModName = "SCAR's pathfinding fix.mod";

Console.WriteLine("OpenConstructionSet Patcher Example");
Console.WriteLine("SCAR's pathfinding fix https://www.nexusmods.com/kenshi/mods/602");
Console.WriteLine();

var installation = await SelectInstallation();

Console.WriteLine();

Console.Write("Reading load order... ");

var baseMods = await ModsToPatch();

Console.WriteLine("done");

Console.Write("Loading data... ");

var (waterAvoidance, pathfindAcceleration, version) = await ReadScarsMod();

var context = await BuildModContext();

Console.WriteLine("done");

// Get all races where editor limits are set i.e. it is not an animal race
var races = context.Items.OfType(ItemType.Race).Where(race => race.Values.TryGetValue("editor limits", out var value)
                                                              && value is FileValue file
                                                              && !string.IsNullOrEmpty(file.Path));

foreach (var race in races)
{
    Console.WriteLine("Updating " + race.Name);
    race.Values["pathfind acceleration"] = pathfindAcceleration;

    // avoid changing for races that like water
    if (race.Values.ContainsKey("water avoidance") && (float)race.Values["water avoidance"] > 0)
    {
        race.Values["water avoidance"] = waterAvoidance;
    }
}

Console.Write("Saving... ");

await context.SaveAsync();

Console.WriteLine("done");

Console.Write("Adding patch to end of load order... ");

var enabledMods = (await installation.ReadEnabledModsAsync()).ToList();

// Remove this mod and then add to the end of the load order
enabledMods.RemoveAll(s => s == ModFileName);
enabledMods.Add(ModFileName);

await installation.WriteEnabledModsAsync(enabledMods);

Console.WriteLine("done");
Console.ReadKey();

async Task<IInstallation> SelectInstallation()
{
    var installations = await new InstallationService().DiscoverAllInstallationsAsync().ToDictionaryAsync(i => i.Identifier);

    if (installations.Count == 0)
    {
        Error("Unable to find game");

#pragma warning disable CS8603 // Possible null reference return.
        return null;
#pragma warning restore CS8603 // Possible null reference return.
    }
    else if (installations.Count == 1)
    {
        // One installation so use it
        return installations.Values.First();
    }
    else
    {
        // Display the installations to the user
        var keys = installations.Keys.ToList();

        Console.WriteLine("Multiple installations found");

        for (var i = 0; i < keys.Count; i++)
        {
            Console.WriteLine($"{i + 1} - {keys[i]}");
        }

        Console.Write("Please select which to use: ");

        // Get the user to chose
        var selection = keys[int.Parse(Console.ReadLine() ?? "1") - 1];

        Console.WriteLine($"Using the {selection} installation");

        return installations[selection];
    }
}

void Error(string message)
{
    Console.WriteLine(message);
    Console.ReadKey();
    Environment.Exit(1);
}

async Task<(float waterAvoidance, float pathFindAcceleration, int version)> ReadScarsMod()
{
    if (!installation.Mods.TryFind(ReferenceModName, out var referenceMod))
    {
        // Not found
        Error($"Unable to find {ReferenceModName}");
        return (0, 0, 0);
    }

    ModFileData referenceData;

    try
    {
        referenceData = await referenceMod.ReadDataAsync();
    }
    catch (Exception ex)
    {
        Error($"Unable to load {ReferenceModName}{Environment.NewLine}Error: {ex}");
        return (0, 0, 0);
    }

    // Extract core values from the Greenlander race item
    var greenlander = referenceData.Items.Find(i => i.Name == "Greenlander");

    if (greenlander is null)
    {
        Error("Could not find Greenlander");
        return (0, 0, 0);
    }

    var pathfindAcceleration = greenlander.Values["pathfind acceleration"];
    var waterAvoidance = greenlander.Values["water avoidance"];

    return ((float)waterAvoidance, (float)pathfindAcceleration, referenceData.Header.Version);
}

async Task<IModContext> BuildModContext()
{
    // Build mod
    var header = new Header(version,
                            "LMayDev",
                            "OpenConstructionSet Compatibility patch to apply core values from SCAR's pathfinding fix to custom races");
    header.References.Add(ReferenceModName);
    header.Dependencies.AddRange(baseMods);

    var options = new ModContextOptions(ModFileName,
        installation: installation,
        baseMods: baseMods,
        header: header,
        throwIfMissing: false);

    return await new ContextBuilder().BuildAsync(options);
}

async Task<List<string>> ModsToPatch()
{
    var mods = new List<string>(await installation.ReadEnabledModsAsync());

    // Don't patch ourselves or SCAR's mod
    mods.Remove(ModFileName);
    mods.Remove(ReferenceModName);

    if (mods.Count == 0)
    {
        // No mods found to patch
        Error($"failed!{Environment.NewLine}No mods found to patch");
        return new();
    }

    return mods;
}