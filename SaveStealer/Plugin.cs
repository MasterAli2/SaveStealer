using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using LethalAPI.LibTerminal;
using LethalAPI.LibTerminal.Attributes;
using LethalAPI.LibTerminal.Models;

using System;


namespace SaveStealer
{
    [BepInPlugin(GUID, NAME, VERSION)]

    [BepInDependency("LethalAPI.Terminal")]
    public class SaveStealerBase : BaseUnityPlugin
    {
        public const string GUID = "MasterAli2.SaveStealer";
        public const string NAME = "Save Stealer";
        public const string VERSION = "0.1.0";

        public static SaveStealerBase Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;

        internal TerminalModRegistry Terminal;


        private string TempFileName = "SaveStealerTempFile";

        void Awake()
        {

            Logger = base.Logger;
            Instance = this;

            Terminal = TerminalRegistry.CreateTerminalRegistry();
            Terminal.RegisterFrom(this);

            Logger.LogInfo($"{GUID} v{VERSION} has loaded!");
        }



        [TerminalCommand("steal")]
        public string StealCommand([RemainingText] string text) { return Steal(text); }

        private string Steal(string text = "")
        {
            try
            {
                if (StartOfRound.Instance.shipHasLanded)
                {
                    return "Ship must be in orbit";
                }


                // Make sure GameNetowrkManager is initialised or we cant save
                GameNetworkManager GNM = GameNetworkManager.Instance;
                if (GNM == null)
                {
                    Logger.LogError("GameNetworkManager not found");
                    return "GameNetworkManager not found";
                }



                // some variables so we can change the vars in the classes and revert them back later
                bool oldIsHostingGame = GNM.isHostingGame;
                string oldSaveFileName = GNM.currentSaveFileName;
                int oldMaxItemCap = StartOfRound.Instance.maxShipItemCapacity;

                string SaveFileName = "";

                if (int.TryParse(text, out int num) == false)
                {
                    return "A valid save number must be given";
                }

                SaveFileName = "LCSaveFile" + num;

                /*
                if (!ES3.FileExists("LCSaveFile" + num)) return "Specified Save file does not exist";
                else SaveFileName = "LCSaveFile" + num;
                */

                // set a variable so the save dosent get interupted by the host check
                GNM.isHostingGame = true;

                // sets the save file to a temp file
                GNM.currentSaveFileName = TempFileName;

                // sets the item limit to infinite so all items save
                StartOfRound.Instance.maxShipItemCapacity = 999;
                try
                {
                    GNM.SaveGame();
                }
                catch (Exception e)
                {
                    // blame the player
                    Logger.LogError(e);
                    return "Saving Failed:\n" + e;
                }

                // revert those variable chnages
                GNM.isHostingGame = oldIsHostingGame;
                GNM.currentSaveFileName = oldSaveFileName;
                StartOfRound.Instance.maxShipItemCapacity = oldMaxItemCap;

                try
                {
                    ES3.RenameFile(TempFileName, SaveFileName);
                }
                catch (Exception e)
                {
                    // blame the player
                    Logger.LogError(e);
                    return "Saving Failed:\n" + e;
                }

                
                 



                // yippe
                return $"Copied current save as {SaveFileName}";

            }
            catch (Exception e)
            {
                // blame the player
                Logger.LogError(e);
                return "Saving Unexpectedly Failed:\n" + e;
            }
        }





    }
}

