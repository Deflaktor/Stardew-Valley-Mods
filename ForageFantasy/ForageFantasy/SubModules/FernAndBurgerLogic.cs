﻿namespace ForageFantasy
{
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.TerrainFeatures;
    using System.Collections.Generic;
    using System.Linq;

    internal class FernAndBurgerLogic
    {
        public static bool CanEdit<T>(IAssetInfo asset, ForageFantasyConfig config)
        {
            if (config.CommonFiddleheadFern)
            {
                if (asset.AssetNameEquals("Data/Locations"))
                {
                    return true;
                }
                else if (asset.AssetNameEquals("Data/CraftingRecipes"))
                {
                    return true;
                }
            }

            if (config.ForageSurvivalBurger)
            {
                if (asset.AssetNameEquals("Data/CookingRecipes"))
                {
                    return true;
                }
                else if (asset.AssetNameEquals("Data/CraftingRecipes"))
                {
                    return true;
                }
            }

            return false;
        }

        public static void Edit<T>(IAssetData asset, ForageFantasyConfig config)
        {
            if (config.CommonFiddleheadFern)
            {
                if (asset.AssetNameEquals("Data/CraftingRecipes"))
                {
                    IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                    data["Wild Seeds (Su)"] = "396 1 398 1 402 1 259 1/Field/496 10/false/Foraging 4";
                }

                if (asset.AssetNameEquals("Data/Locations"))
                {
                    IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                    var keys = data.Keys.ToList();

                    for (int i = 0; i < keys.Count; i++)
                    {
                        string location = keys[i];
                        string[] fields = data[location].Split('/');

                        switch (location)
                        {
                            case "BusStop":
                                fields[1] = "396 .6 398 .6 402 .6";
                                break;

                            case "Forest":
                                fields[1] = "396 .8 398 .8 259 .8";
                                break;

                            case "Mountain":
                                fields[1] = "396 .7 398 .7 259 .8";
                                break;

                            case "Backwoods":
                                fields[1] = "396 .7 398 .7 259 .8";
                                break;

                            case "Railroad":
                                fields[1] = "396 .6 398 .6 402 .6";
                                break;

                            case "Woods":
                                fields[1] = "259 .7 420 .7";
                                break;
                        }

                        data[location] = string.Join("/", fields);
                    }
                }
            }

            if (config.ForageSurvivalBurger)
            {
                if (asset.AssetNameEquals("Data/CookingRecipes"))
                {
                    IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                    data.Remove("Survival Burger");
                    data.Add("Survival Burger (Sp)", "216 1 16 1 20 1 22 1/70 1/241 2/s Foraging 2/Survival Burger (Sp)");
                    data.Add("Survival Burger (Su)", "216 1 398 1 396 1 259 1/70 1/241 2/s Foraging 2/Survival Burger (Su)");
                    data.Add("Survival Burger (Fa)", "216 1 404 1 406 1 408 1/70 1/241 2/s Foraging 2/Survival Burger (Fa)");
                    data.Add("Survival Burger (Wi)", "216 1 412 1 414 1 416 1/70 1/241 2/s Foraging 2/Survival Burger (Wi)");
                }

                if (asset.AssetNameEquals("Data/CraftingRecipes"))
                {
                    IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                    data.Add("Survival Burger (Sp)", "216 1 16 1 20 1 22 1/Field/241/false/s Foraging 2/Survival Burger (Sp)");
                    data.Add("Survival Burger (Su)", "216 1 398 1 396 1 259 1/Field/241/false/s Foraging 2/Survival Burger (Su)");
                    data.Add("Survival Burger (Fa)", "216 1 404 1 406 1 408 1/Field/241/false/s Foraging 2/Survival Burger (Fa)");
                    data.Add("Survival Burger (Wi)", "216 1 412 1 414 1 416 1/Field/241/false/s Foraging 2/Survival Burger (Wi)");
                }
            }
        }

        public static void OnDayStarted(ForageFantasy mod, GameLocation location)
        {
            if (!Context.IsMainPlayer)
            {
                return;
            }

            if (mod.Config.CommonFiddleheadFern && Game1.currentSeason == "summer")
            {
                foreach (var vec in mod.ForageLocationsToWatch)
                {
                    if (location.objects.ContainsKey(vec))
                    {
                        Object o = location.objects[vec];

                        if (o.IsSpawnedObject && o.CanBeGrabbed)
                        {
                            RerandomizeWildSeedForage(vec, location);
                        }
                    }
                }
            }
        }

        public static void OnDayEnded(ForageFantasy mod)
        {
            if (!Context.IsMainPlayer || !mod.Config.CommonFiddleheadFern || Game1.currentSeason != "summer")
            {
                return;
            }

            mod.ForageLocationsToWatch.Clear();

            foreach (var location in Game1.locations)
            {
                foreach (var terrainfeature in location.terrainFeatures.Pairs)
                {
                    switch (terrainfeature.Value)
                    {
                        case HoeDirt hoeDirt:
                            if (hoeDirt.crop != null)
                            {
                                Crop crop = hoeDirt.crop;

                                if (crop.isWildSeedCrop())
                                {
                                    mod.ForageLocationsToWatch.Add(terrainfeature.Key);
                                }
                            }

                            break;
                    }
                }
            }
        }

        public static void ChangeBundle(ForageFantasy mod)
        {
            if (!mod.Config.CommonFiddleheadFern)
            {
                return;
            }

            Dictionary<string, string> bundleData = Game1.netWorldState.Value.BundleData;

            // Summer Foraging
            string key = "Crafts Room/14";

            string[] bundle = bundleData[key].Split('/');

            if (!bundle[2].Contains("259 1 0"))
            {
                bundle[2] += " 259 1 0";
            }

            bundleData[key] = string.Join("/", bundle);
        }

        private static void RerandomizeWildSeedForage(Vector2 vec, GameLocation location)
        {
            location.objects.Remove(vec);
            location.objects.Add(vec, new Object(vec, GetWildSeedSummerForage(), 1) { IsSpawnedObject = true, CanBeGrabbed = true });
        }

        private static int GetWildSeedSummerForage()
        {
            int ran = Game1.random.Next(4);

            switch (ran)
            {
                case 0:
                    return 259;

                case 1:
                    return 396;

                case 2:
                    return 398;

                default:
                    return 402;
            }
        }
    }
}