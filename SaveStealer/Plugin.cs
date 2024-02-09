using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using LethalAPI.LibTerminal;
using LethalAPI.LibTerminal.Attributes;
using LethalAPI.LibTerminal.Models;

using System;


namespace SaveStealer
{
    [BepInPlugin(modGUID, modName, modVersion)]

    [BepInDependency("Pooble-LCBetterSaves-1.7.3")]
    [BepInDependency("LethalAPI-LethalAPI_Terminal-1.0.1")]
    public class SaveStealerBase : BaseUnityPlugin
    {
        public const string modGUID = "MasterAli2.SaveStealer";
        public const string modName = "Save Stealer";
        public const string modVersion = "0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static SaveStealerBase instance;

        internal ManualLogSource mls;

        private TerminalModRegistry TerminalReg;


        

        void Awake()
        {

            try
            {
                //define instance even tho we dont use it.
                if (instance == null)
                {
                    instance = this;
                }

                //define mls
                mls = this.Logger;





                //define and register terminal command
                TerminalReg = TerminalRegistry.CreateTerminalRegistry();
                TerminalReg.RegisterFrom(this);

                //patch this
                harmony.PatchAll(typeof(SaveStealerBase));

                //confirm that mod loaded succesfully
                mls.LogInfo(modName + "Loaded!");
            }
            catch(Exception E)
            {

                //this is bad
                this.Logger.LogError("Failed to load " + modName + ":\n" + E);
            }


        }


        // define command for when there is not a costom alias
        [TerminalCommand("steal")]
        public string StealerCom() { return Steal(); }

        // define command for when there is a costom alias
        [TerminalCommand("steal")]
        public string StealerCom([RemainingText] string text) { return Steal(text); }


        

        private string Steal(string text = "")
        {

            // make sure ship has not landed or corrupt save (acording to Unique Albino)
            if (StartOfRound.Instance.shipHasLanded)
            {
                return "Ship must be in orbit";
            }


            // Make sure GameNetowrkManager is initialised or we cant save
            GameNetworkManager GNM = GameNetworkManager.Instance;
            if (GNM == null)
            {
                mls.LogError("GameNetworkManager not found");
                return "GameNetworkManager not found";
            }


            // a variable to store file name in case there is a costom alias
            string SaveFileName = "";

            // some variables so we can change the vars in the classes and revert them back later
            bool oldIsHostingGame = GNM.isHostingGame;
            string alias = GNM.steamLobbyName + " copy";
            string oldSaveFileName = GNM.currentSaveFileName;
            int oldMaxItemCap = StartOfRound.Instance.maxShipItemCapacity;

            // check if we have a costom alias
            if (text.Length > 0 || text != "")
            {
                alias = text;
            }

            // find open save slot even tough the 16 slot limit is probably not nessesairy, just to be safe
            for (int i = 1; i < 17; i++)
            {
                if (!ES3.FileExists("LCSaveFile" + i) || ES3.Load<string>("Alias_BetterSaves", "LCSaveFile" + i) == alias)
                {
                    SaveFileName = "LCSaveFile" + i;
                    break;
                }
            }

            // tells the player there is not slot to save in
            if (SaveFileName == "")
            {
                return "Unable to find free space to save copy";
            }

            // set a variable so the save dosent get interupted by the host check
            GNM.isHostingGame = true;

            // sets the save file to an open save slot
            GNM.currentSaveFileName = SaveFileName;

            // sets the item limit to infinite so all items save
            StartOfRound.Instance.maxShipItemCapacity = 999;
            try
            {
                // tries to save and set alias
                GNM.SaveGame();
                ES3.Save("Alias_BetterSaves", alias, SaveFileName);
            }
            catch (Exception e)
            {
                // blame the player
                mls.LogError(e);
                return "Saving Failed: " + e;
            }

            // revert those variable chnages
            GNM.isHostingGame = oldIsHostingGame;
            GNM.currentSaveFileName = oldSaveFileName;
            StartOfRound.Instance.maxShipItemCapacity = oldMaxItemCap;

            // yippe
            return "Copied current save with alias: \"" + alias + "\"";
        }





    }
}
