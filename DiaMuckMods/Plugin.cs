using BepInEx;
using DiaMuckMods.Patches;
using HarmonyLib;
using JetBrains.Annotations;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace DiaMuckMods
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private Rect windowRect = new Rect(20, 20, 500, 400); // Initial position and size of the window
        private bool isDragging = false;
        private Vector2 dragStartPos;
        public ItemManager itemManager;


        Mods mods = new Mods();

        // All of the mods
        private bool showGUI = true;
        private bool immortal = false;
        private bool noclip = false;
        private bool infiniteJumps = false;
        private bool slidSlides = false;
        private bool fly = false;
        private bool flySpeedBoost = false;
        private bool moonGravity = false;
        private bool jupiterGravity = false;
        private bool zeroGravity = false;
        private bool sunGravity = false;
        private bool neverHungry = false;
        private bool neverTired = false;
        private bool speedBoost = false;
        private bool dropLag = false;
        private bool lobbyFiller = false;
        private bool itemFountain = false;
        private bool spamPlayers = false;
        private string chatMessage = "";

        // Mod Options
        public float flySpeed = 40f;
        private int lastPlayerId = 0;

        // Dropdown-related variables
        private bool showItemDropdown = false;
        private string selectedItemName = "Select an Item";
        private List<string> itemNames = new List<string>();
        private Vector2 itemDropdownScrollPos;

        // Define categories
        public enum ModCategory
        {
            Movement,
            Stats,
            Gravity,
            Map,
            Server,
            Player,
            Spawn,
            Teleport,
            Debug,
            // Add more categories here
        }

        public ModCategory currentCategory = ModCategory.Movement;
        private Vector2 categoryScrollPos; // Scroll position for categories

        void OnGUI()
        {
            GUI.color = UnityEngine.Color.white;
            if (showGUI)
            {
                // Draw the window
                windowRect = GUI.Window(0, windowRect, WindowFunction, "Dia Mods");
            }
        }

        void WindowFunction(int windowID)
        {
            // Make the window draggable only when clicking on the title bar
            Rect titleBarRect = new Rect(0, 0, windowRect.width, 20);
            GUI.DragWindow(titleBarRect);

            // Check if the mouse is within the title bar area
            if (titleBarRect.Contains(Event.current.mousePosition))
            {
                // Handle dragging
                if (Event.current.type == EventType.MouseDown)
                {
                    isDragging = true;
                    dragStartPos = Event.current.mousePosition - windowRect.position;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    isDragging = false;
                    Event.current.Use();
                }
            }

            if (isDragging)
            {
                // Update the window's position while dragging
                windowRect.position = Event.current.mousePosition - dragStartPos;
            }

            // Display category buttons with scrolling
            categoryScrollPos = GUILayout.BeginScrollView(categoryScrollPos, GUILayout.Width(100));
            GUILayout.BeginVertical();
            foreach (ModCategory category in Enum.GetValues(typeof(ModCategory)))
            {
                if (GUILayout.Toggle(category == currentCategory, category.ToString(), GUI.skin.button))
                {
                    currentCategory = category;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // Display mods based on the current category
            switch (currentCategory)
            {
                case ModCategory.Movement:
                    DisplayMovementMods();
                    break;
                case ModCategory.Stats:
                    DisplayStatsMods();
                    break;
                case ModCategory.Gravity:
                    DisplayGravityMods();
                    break;
                case ModCategory.Map:
                    DisplayMapMods();
                    break;
                case ModCategory.Server:
                    DisplayServerMods();
                    break;
                case ModCategory.Player:
                    DisplayPlayerMods();
                    break;
                case ModCategory.Spawn:
                    DisplaySpawnMods();
                    break;
                case ModCategory.Teleport:
                    DisplayTeleportMods();
                    break;
                case ModCategory.Debug:
                    DisplayDebugMods();
                    break;
                    // Add cases for other categories here
            }
        }


        void DisplayMovementMods()
        {
            // Icy Floor
            slidSlides = GUI.Toggle(new Rect(120, 30, 140, 20), slidSlides, "Icy Floor");

            // Infinite Jumps
            infiniteJumps = GUI.Toggle(new Rect(120, 50, 140, 20), infiniteJumps, "Infinite Jumps");

            // Fly
            fly = GUI.Toggle(new Rect(120, 70, 140, 20), fly, "Fly");

            // Noclip
            noclip = GUI.Toggle(new Rect(120, 90, 140, 20), noclip, "Noclip");

            string speedBoostText;

            if (speedBoost)
            {
                speedBoostText = "Speedboost: On";
            }
            else
            {
                speedBoostText = "Speedboost: Off";
            }

            if (GUI.Button(new Rect(120, 115, 140, 20), speedBoostText))
            {
                // Find the player GameObject by tag
                GameObject playerGameObject = GameObject.Find("Player");

                if (playerGameObject != null)
                {
                    // Get the PlayerMovement component from the player GameObject
                    PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();

                    if (playerMovement != null)
                    {
                        if (!speedBoost)
                        {
                            // Modify the private field values
                            //Traverse.Create(playerMovement).Field("maxRunSpeed").SetValue(17.5f);
                            Traverse.Create(playerMovement).Field("maxRunSpeed").SetValue(50f);
                            Traverse.Create(playerMovement).Field("maxWalkSpeed").SetValue(35f);

                            speedBoost = true;
                        }
                        else
                        {
                            // Modify the private field values
                            Traverse.Create(playerMovement).Field("maxRunSpeed").SetValue(13f);
                            Traverse.Create(playerMovement).Field("maxWalkSpeed").SetValue(6.5f);

                            speedBoost = false;
                        }
                    }
                    else
                    {
                        Debug.LogError("PlayerMovement component not found on the Player GameObject.");
                    }
                }
                else
                {
                    Debug.LogError("Player GameObject not found.");
                }
            }
        }

        void DisplayStatsMods()
        {
            // Immortality Mod
            immortal = GUI.Toggle(new Rect(120, 30, 140, 20), immortal, "Immortality");

            // Never Hungery
            neverHungry = GUI.Toggle(new Rect(120, 50, 140, 20), neverHungry, "Never Hungry");

            // Never Tired
            neverTired = GUI.Toggle(new Rect(120, 70, 140, 20), neverTired, "Never Tired");

            if (GUI.Button(new Rect(120, 100, 140, 20), "Rich Man"))
            {
                var kvp = ItemManager.Instance.allItems[3];
                InventoryItem itifno = kvp;
                kvp.max = 999999999;
                Mods.GiveItem(3, 999999999);
            }
        }

        void DisplayGravityMods()
        {
            // Moon Gravity
            moonGravity = GUI.Toggle(new Rect(120, 30, 140, 20), moonGravity, "Moon Gravity");

            // Jupiter Gravity
            jupiterGravity = GUI.Toggle(new Rect(120, 50, 140, 20), jupiterGravity, "Jupiter Gravity");

            // Zero Gravity
            zeroGravity = GUI.Toggle(new Rect(120, 70, 140, 20), zeroGravity, "Zero Gravity");

            // Sun Gravity
            sunGravity = GUI.Toggle(new Rect(120, 90, 140, 20), sunGravity, "Sun Gravity");
        }

        void DisplayMapMods()
        {
            if (GUI.Button(new Rect(120, 30, 140, 20), "Reload Map"))
            {
                SceneManager.LoadScene("GameAfterLobby");
            }
            if (GUI.Button(new Rect(120, 60, 140, 20), "Map in Map"))
            {
                SceneManager.LoadScene("GameAfterLobby", LoadSceneMode.Additive);
            }
            if (GUI.Button(new Rect(120, 90, 140, 20), "Big Boat"))
            {
                mods.SizeChanger("Boat", new Vector3(5f, 5f, 5f));
            }
            if (GUI.Button(new Rect(120, 120, 140, 20), "Pickup All Pickups"))
            {
                PickupInteract[] pickupInteract = FindObjectsOfType<PickupInteract>();

                foreach (PickupInteract pickup in pickupInteract)
                {
                    pickup.Interact();
                }
            }
            if (GUI.Button(new Rect(120, 150, 160, 20), "Pickup All Pickups (CS)"))
            {
                PickupInteract[] pickupInteract = FindObjectsOfType<PickupInteract>();

                foreach (PickupInteract pickup in pickupInteract)
                {
                    pickup.LocalExecute();
                }
            }
            if (GUI.Button(new Rect(120, 180, 140, 20), "Large Pickups"))
            {
                PickupInteract[] pickupInteract = FindObjectsOfType<PickupInteract>();

                foreach (PickupInteract pickup in pickupInteract)
                {
                    pickup.amount = 999999999;
                }
            }
            if (GUI.Button(new Rect(120, 210, 140, 20), "Destroy Pickups"))
            {
                PickupInteract[] pickupInteract = FindObjectsOfType<PickupInteract>();

                foreach (PickupInteract pickup in pickupInteract)
                {
                    pickup.RemoveObject();
                }
            }
            if (GUI.Button(new Rect(120, 240, 140, 20), "Bring Rocks (CS)"))
            {
                HitableRock[] pickupInteract = FindObjectsOfType<HitableRock>();

                Transform playerTransform = GameObject.Find("Player").transform;

                Vector3 spawnPostion = new Vector3(playerTransform.position.x+2f, playerTransform.position.y, playerTransform.position.z);

                foreach (HitableRock pickup in pickupInteract)
                {
                    pickup.gameObject.transform.position = spawnPostion;
                }
            }

            if (GUI.Button(new Rect(120, 270, 140, 20), "Bring Trees (CS)"))
            {
                Transform playerTransform = GameObject.Find("Player").transform;

                Vector3 spawnPostion = new Vector3(playerTransform.position.x + 2f, playerTransform.position.y, playerTransform.position.z);

                HitableTree[] trees = FindObjectsOfType<HitableTree>();

                foreach (HitableTree tree in trees)
                {
                    tree.gameObject.transform.position = spawnPostion;
                }
            }

            if (GUI.Button(new Rect(120, 300, 140, 20), "Bring Chests (CS)"))
            {
                HitableChest[] pickupInteract = FindObjectsOfType<HitableChest>();

                Transform playerTransform = GameObject.Find("Player").transform;

                Vector3 spawnPostion = new Vector3(playerTransform.position.x + 2f, playerTransform.position.y, playerTransform.position.z);

                foreach (HitableChest pickup in pickupInteract)
                {
                    pickup.gameObject.transform.position = spawnPostion;
                }
            }

            if (GUI.Button(new Rect(120, 330, 140, 20), "Bring Dropped Items (CS)"))
            {
                Item[] pickupInteract = FindObjectsOfType<Item>();

                Transform playerTransform = GameObject.Find("Player").transform;

                Vector3 spawnPostion = new Vector3(playerTransform.position.x + 2f, playerTransform.position.y, playerTransform.position.z);

                foreach (Item pickup in pickupInteract)
                {
                    pickup.gameObject.transform.position = spawnPostion;
                }
            }
        }

        void DisplayServerMods()
        {
            // Display text field for input
            chatMessage = GUI.TextField(new Rect(120, 30, 140, 20), chatMessage);

            // Display a button for submitting
            if (GUI.Button(new Rect(120, 60, 120, 20), "Send in Chat (CS)"))
            {
                ChatBox.Instance.AppendMessage(-1, string.Format(chatMessage), "");
            }

            // Free Chests
            if (GUI.Button(new Rect(120, 90, 140, 20), "Free Chests"))
            {
                Harmony harmony = new Harmony("NoCost");
                MethodInfo methodInfo = AccessTools.Method(typeof(GameManager), "ChestPriceMultiplier", null, null);
                MethodInfo methodInfo2 = AccessTools.Method(typeof(CostChangeChest), "ChestPriceMultiplier_Patch", null, null);
                harmony.Patch(methodInfo, null, new HarmonyMethod(methodInfo2), null, null, null);
            }

            // Force Win
            if (GUI.Button(new Rect(120, 120, 140, 20), "Force Win"))
            {
                GameObject playerObject = GameObject.Find("Player");

                Transform playerTransform = playerObject.transform;

                Vector3 spawnPostion = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z);

                Mods.SpawnMob("Bob", spawnPostion);

                Mob[] mobs = FindObjectsOfType<Mob>();

                foreach (Mob mob in mobs)
                {
                    GameObject mobGameObject = mob.gameObject;

                    Destroy(mobGameObject);
                }
            }
            if (GUI.Button(new Rect(120, 150, 140, 20), "Free/Open Revive"))
            {
                RespawnTotemUI[] respawnTotemUIs = FindObjectsOfType<RespawnTotemUI>();

                foreach (RespawnTotemUI respawnTotemUI in respawnTotemUIs)
                {
                    respawnTotemUI.basePrice = 0;
                    respawnTotemUI.Show();
                }
            }
            if (GUI.Button(new Rect(120, 180, 140, 20), "Mark Boat"))
            {
                Boat[] boats = FindObjectsOfType<Boat>();

                foreach (Boat boat in boats)
                {
                    boat.MarkShip();
                }
            }
            if (GUI.Button(new Rect(120, 210, 140, 20), "Leave Island"))
            {
                Boat[] boats = FindObjectsOfType<Boat>();

                foreach (Boat boat in boats)
                {
                    boat.LeaveIsland();
                }
            }
            if (GUI.Button(new Rect(120, 240, 140, 20), "Summon All"))
            {
                ShrineBoss[] shrineBosses = FindObjectsOfType<ShrineBoss>();
                ShrineGuardian[] shrineGuardians = FindObjectsOfType<ShrineGuardian>();
                ShrineInteractable[] shrineInteractables = FindObjectsOfType<ShrineInteractable>();

                foreach (ShrineBoss shrineBoss in shrineBosses)
                {
                    shrineBoss.AllExecute();
                }
                foreach (ShrineGuardian shrineGuardian in shrineGuardians)
                {
                    shrineGuardian.AllExecute();
                }
                foreach (ShrineInteractable shrineInteractable in shrineInteractables)
                {
                    shrineInteractable.AllExecute();
                }
            }
            if (GUI.Button(new Rect(120, 270, 140, 20), "Summon All Local"))
            {
                ShrineBoss[] shrineBosses = FindObjectsOfType<ShrineBoss>();
                ShrineGuardian[] shrineGuardians = FindObjectsOfType<ShrineGuardian>();
                ShrineInteractable[] shrineInteractables = FindObjectsOfType<ShrineInteractable>();

                foreach (ShrineBoss shrineBoss in shrineBosses)
                {
                    shrineBoss.LocalExecute();
                }
                foreach (ShrineGuardian shrineGuardian in shrineGuardians)
                {
                    shrineGuardian.LocalExecute();
                }
                foreach (ShrineInteractable shrineInteractable in shrineInteractables)
                {
                    shrineInteractable.LocalExecute();
                }
            }
            if (GUI.Button(new Rect(120, 300, 140, 20), "Add Player (CS)"))
            {
                lastPlayerId++;

                Vector3 addPlayerPostion = new Vector3(GameObject.Find("Player").transform.position.x, GameObject.Find("Player").transform.position.y, GameObject.Find("Player").transform.position.z);

                UnityEngine.Color color = Mods.GetRandomColor();

                GameManager.instance.SpawnPlayer(lastPlayerId, chatMessage, Mods.GetRandomColor(), addPlayerPostion, Mods.GetRandomNumber(0, 100));
            }
            spamPlayers = GUI.Toggle(new Rect(120, 360, 140, 20), spamPlayers, "Spam Players (CS)");
            if (GUI.Button(new Rect(120, 330, 140, 20), "Kill Others (CS)"))
            {
                PlayerManager[] players = FindObjectsOfType<PlayerManager>();

                PlayerManager playerManagerSelf = new PlayerManager();

                foreach (PlayerManager playerManager in players)
                {
                    if (playerManagerSelf.id != playerManager.id)
                    {
                        GameManager.instance.KillPlayer(playerManager.id, playerManager.gameObject.transform.position);
                        ServerSend.PlayerDied(playerManager.id, playerManager.gameObject.transform.position, playerManager.gameObject.transform.position, playerManager.id);
                    }
                }
            }
            if (GUI.Button(new Rect(270, 30, 140, 20), "Kick Others (Borked)"))
            {
                PlayerManager[] players = FindObjectsOfType<PlayerManager>();

                PlayerManager playerManagerSelf = new PlayerManager();

                foreach (PlayerManager playerManager in players)
                {
                    if (playerManagerSelf.id != playerManager.id)
                    {
                        GameManager.instance.KickPlayer(playerManager.name);
                    }
                }
            }
            if (GUI.Button(new Rect(270, 60, 140, 20), "Broken Start"))
            {
                GameObject.Find("=====DONTDESTROY=====/Lobby").GetComponent<SteamLobby>().StartGame();
            }
            if (GUI.Button(new Rect(270, 90, 140, 20), "Close Lobby"))
            {
                GameObject.Find("=====DONTDESTROY=====/Lobby").GetComponent<SteamLobby>().CloseLobby();
            }
            if (GUI.Button(new Rect(270, 120, 140, 20), "Leave Lobby"))
            {
                GameObject.Find("=====DONTDESTROY=====/Lobby").GetComponent<SteamManager>().leaveLobby();
            }
        }

        void DisplayPlayerMods()
        {
            // Self Revive
            if (GUI.Button(new Rect(120, 30, 140, 20), "Revive"))
            {
                Mods.Revive();
            }

            if (GUI.Button(new Rect(120, 60, 140, 20), "Big Stacks"))
            {
                foreach (var kvp in ItemManager.Instance.allItems)
                {
                    InventoryItem itifno = kvp.Value;

                    itifno.max = 999999999;
                }
            }

            if (GUI.Button(new Rect(120, 90, 140, 20), "Best Load Out"))
            {
                GameObject playerObject = GameObject.Find("Player");

                Transform playerTransform = playerObject.transform;

                Vector3 spawnPostion = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z);

                Mods.GiveItem(9, 1);
                Mods.GiveItem(16, 1);
                Mods.GiveItem(23, 1);
                Mods.GiveItem(30, 1);
                Mods.GiveItem(71, 69);
                Mods.GiveItem(128, 1);
                Mods.GiveItem(118, 1);
                Mods.GiveItem(108, 1);
            }

            if (GUI.Button(new Rect(120, 120, 140, 20), "Get All Gems"))
            {
                GameObject playerObject = GameObject.Find("Player");

                Transform playerTransform = playerObject.transform;

                Vector3 spawnPostion = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z);

                Mods.GiveItem(103, 1);
                Mods.GiveItem(104, 1);
                Mods.GiveItem(105, 1);
                Mods.GiveItem(106, 1);
                Mods.GiveItem(107, 1);
            }
            if (GUI.Button(new Rect(120, 150, 140, 20), "Everything OP"))
            {
                foreach (var kvp in ItemManager.Instance.allItems)
                {
                    InventoryItem itifno = kvp.Value;

                    itifno.max = 10000;
                    itifno.tier = 10000;
                    itifno.armor = 10000;
                    itifno.sharpness = 10000;
                    itifno.attackRange = 10000;
                    itifno.attackSpeed = 10000;
                    itifno.heal = 10000;
                    itifno.hunger = 10000;
                    itifno.stamina = 10000;
                }
            }
            if (GUI.Button(new Rect(120, 180, 180, 20), "Everything OP Lower Speed"))
            {
                foreach (var kvp in ItemManager.Instance.allItems)
                {
                    InventoryItem itifno = kvp.Value;

                    itifno.max = 10000;
                    itifno.tier = 10000;
                    itifno.armor = 10000;
                    itifno.sharpness = 10000;
                    itifno.attackRange = 10000;
                    itifno.attackSpeed = 10;
                    itifno.heal = 10000;
                    itifno.hunger = 10000;
                    itifno.stamina = 10000;
                }
            }
            if (GUI.Button(new Rect(120, 210, 140, 20), "KYS NOW"))
            {
                PlayerStatus.Instance.Damage(-100);
                PlayerManager playerManger = new PlayerManager();

                playerManger.dead = true;
            }
        }

        void DisplaySpawnMods()
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject == null)
            {
                GUI.TextField(new Rect(120, 30, 300, 20), "Can't Spawn Things until you are in a lobby");
            }
            else
            {
                Transform playerTransform = playerObject.transform;

                Vector3 spawnPostion = new Vector3(playerTransform.position.x+3f, playerTransform.position.y, playerTransform.position.z);

                // Button Spawners
                if (GUI.Button(new Rect(120, 30, 140, 20), "Spawn Bob"))
                {
                    Mods.SpawnMob("Bob", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 60, 140, 20), "Spawn Cow"))
                {
                    Mods.SpawnMob("Cow", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 90, 140, 20), "Spawn Fire Dave"))
                {
                    Mods.SpawnMob("Dave (Fire)", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 120, 140, 20), "Spawn Lightning Dave"))
                {
                    Mods.SpawnMob("Dave (Lightning)", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 150, 140, 20), "Spawn Water Dave"))
                {
                    Mods.SpawnMob("Dave (Water)", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 180, 140, 20), "Spawn Lil Dave"))
                {
                    Mods.SpawnMob("lil Dave", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 210, 140, 20), "Spawn Goblin"))
                {
                    Mods.SpawnMob("Goblin", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 240, 140, 20), "Spawn Stone Golem"))
                {
                    Mods.SpawnMob("Stone Golem", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 270, 140, 20), "Spawn Gronk"))
                {
                    Mods.SpawnMob("Gronk", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 300, 140, 20), "Spawn Wyvern"))
                {
                    Mods.SpawnMob("Wyvern", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 330, 140, 20), "Spawn Big Chunk"))
                {
                    Mods.SpawnMob("Big Chunk", spawnPostion);
                }
                if (GUI.Button(new Rect(120, 360, 140, 20), "Spawn Wolf"))
                {
                    Mods.SpawnMob("Wolf", spawnPostion);
                }
                if (GUI.Button(new Rect(270, 30, 140, 20), "Spawn Guardians"))
                {
                    Mods.SpawnGuardianMob(-1, spawnPostion);
                    Mods.SpawnGuardianMob(0, spawnPostion);
                    Mods.SpawnGuardianMob(1, spawnPostion);
                    Mods.SpawnGuardianMob(2, spawnPostion);
                    Mods.SpawnGuardianMob(3, spawnPostion);
                    Mods.SpawnGuardianMob(4, spawnPostion);
                    Mods.SpawnGuardianMob(5, spawnPostion);
                    Mods.SpawnGuardianMob(6, spawnPostion);
                }
                if (GUI.Button(new Rect(270, 60, 140, 20), "Spawn Woodman"))
                {
                    Mods.SpawnMob("Woodman", spawnPostion);
                }
                if (GUI.Button(new Rect(270, 90, 140, 20), "Spawn Chief"))
                {
                    Mods.SpawnMob("Chief", spawnPostion);
                }
                if (GUI.Button(new Rect(270, 120, 120, 20), "Kill All (CS)"))
                {
                    Mob[] mobs = FindObjectsOfType<Mob>();

                    foreach (Mob mob in mobs)
                    {
                        Mods.MobIdsCool++;

                        GameObject mobGameObject = mob.gameObject;

                        Destroy(mobGameObject);
                    }
                }
                if (GUI.Button(new Rect(270, 150, 140, 20), "Kill All (Borked) (SS)"))
                {
                    //for (int i = 0; i < MobManager.Instance.GetNextId(); i++)
                    //{
                    //MobManager.Instance.RemoveMob(i);
                    //}

                    Mob[] mobs = FindObjectsOfType<Mob>();

                    Vector3 newKillSpot = new Vector3(99999999f, 99999999f, 99999999f);

                    foreach (Mob mob in mobs)
                    {
                        mob.SetPosition(newKillSpot);
                    }
                }
            }
            // The spammers
            //moonGravity = GUI.Toggle(new Rect(250, 30, 140, 20), moonGravity, "Moon Gravity");
        }
        void DisplayTeleportMods()
        {
            float YOffest = 20f;

            GameObject playerObject = GameObject.Find("Player");
            if (playerObject == null)
            {
                GUI.TextField(new Rect(120, 30, 300, 20), "Can't Teleport until you are in a lobby");
            }
            else
            {
                Transform playerTransform = playerObject.transform;

                if (GUI.Button(new Rect(120, 30, 140, 20), "TP to Boat"))
                {
                    Boat[] boats = FindObjectsOfType<Boat>();

                    foreach (Boat boat in boats)
                    {
                        Vector3 teleportTest = new Vector3(boat.gameObject.transform.position.x, boat.gameObject.transform.position.y + YOffest, boat.gameObject.transform.position.z);

                        playerTransform.position = teleportTest;
                    }
                }

                if (GUI.Button(new Rect(120, 60, 140, 20), "TP to Next Ore"))
                {
                    HitableRock[] pickupInteract = FindObjectsOfType<HitableRock>();

                    foreach (HitableRock pickup in pickupInteract)
                    {
                        Vector3 teleportTest = new Vector3(pickup.gameObject.transform.position.x, pickup.gameObject.transform.position.y + YOffest, pickup.gameObject.transform.position.z);

                        playerTransform.position = teleportTest;
                        break;
                    }
                }

                if (GUI.Button(new Rect(120, 90, 140, 20), "TP to Next Tree"))
                {
                    HitableTree[] pickupInteract = FindObjectsOfType<HitableTree>();

                    foreach (HitableTree pickup in pickupInteract)
                    {
                        Vector3 teleportTest = new Vector3(pickup.gameObject.transform.position.x, pickup.gameObject.transform.position.y + YOffest, pickup.gameObject.transform.position.z);

                        playerTransform.position = teleportTest;

                        break;
                    }
                }
                if (GUI.Button(new Rect(120, 120, 140, 20), "TP to Next Mob"))
                {
                    Mob[] pickupInteract = FindObjectsOfType<Mob>();

                    foreach (Mob pickup in pickupInteract)
                    {
                        Vector3 teleportTest = new Vector3(pickup.gameObject.transform.position.x, pickup.gameObject.transform.position.y + YOffest, pickup.gameObject.transform.position.z);

                        playerTransform.position = teleportTest;

                        break;
                    }
                }
                if (GUI.Button(new Rect(120, 150, 140, 20), "TP to Next Chest"))
                {
                    HitableChest[] pickupInteract = FindObjectsOfType<HitableChest>();

                    foreach (HitableChest pickup in pickupInteract)
                    {
                        Vector3 teleportTest = new Vector3(pickup.gameObject.transform.position.x, pickup.gameObject.transform.position.y + YOffest, pickup.gameObject.transform.position.z);

                        playerTransform.position = teleportTest;

                        break;
                    }
                }
                if (GUI.Button(new Rect(270, 150, 140, 20), "Destroy Next Chest"))
                {
                    HitableChest[] pickupInteract = FindObjectsOfType<HitableChest>();

                    foreach (HitableChest pickup in pickupInteract)
                    {
                        Destroy(pickup);
                        Destroy(pickup.gameObject);
                        break;
                    }
                }
            }
        }

        void DisplayDebugMods()
        {
            Filestuff filestuff = new Filestuff();

            if (GUI.Button(new Rect(120, 30, 140, 20), "Get Mob Info"))
            {
                MobType mobType = ScriptableObject.CreateInstance<MobType>();

                string text = "Mob Info\n-------------------";

                // Find the correct MobType by name
                foreach (MobType mober in MobSpawner.Instance.allMobs)
                {
                    text += "\nName: " + mober.name + "\nID: " + mober.id+"\nKnockback: "+mober.knockbackThreshold+ "\n-------------------";
                }
                filestuff.WriteToFile("mob_info.txt", text);
            }

            if (GUI.Button(new Rect(120, 60, 140, 20), "Get Item Info"))
            {
                //InventoryItem inventoryItem = ScriptableObject.CreateInstance<InventoryItem>();

                string text = "Item Info\n-------------------";

                // Find the correct MobType by name
                foreach (var kvp in ItemManager.Instance.allItems)
                {
                    InventoryItem itifno = kvp.Value;

                    text += "\nName: "+ itifno.name+ "\nRarity: " + itifno.rarity+"\nID: "+ itifno.id + "\n-------------------";
                }
                filestuff.WriteToFile("item_info.txt", text);
            }

            if (GUI.Button(new Rect(120, 90, 140, 20), "Get Powerup Info"))
            {
                //InventoryItem inventoryItem = ScriptableObject.CreateInstance<InventoryItem>();

                string text = "Powerup Info\n-------------------";

                // Find the correct MobType by name
                foreach (var kvp in ItemManager.Instance.allPowerups)
                {
                    Powerup itifno = kvp.Value;

                    text += "\nName: "+ itifno.name+"\nDescription: "+ itifno.description+"\nID: "+itifno.id + "\n-------------------";
                }
                filestuff.WriteToFile("powerup_info.txt", text);
            }
        }

        void Start()
        {

        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            /* Code here runs after the game initializes (i.e. GorillaLocomotion.Player.Instance != null) */
        }

        void Update()
        {
            // Keybinds
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showGUI = !showGUI;
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (!Cursor.visible) // If cursor is currently invisible
                {
                    Cursor.visible = true; // Make cursor visible
                    Cursor.lockState = CursorLockMode.None; // Unlock cursor
                }
                else // If cursor is currently visible
                {
                    Cursor.visible = false; // Hide cursor
                    Cursor.lockState = CursorLockMode.Locked; // Lock cursor
                }
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                fly = !fly;
            }

            // Mod Functions

            // Immortal
            if (immortal)
            {
                try
                {
                    if (PlayerStatus.Instance != null)
                    {
                        PlayerStatus.Instance.hp = 100;
                    }
                    else
                    {
                        Debug.LogWarning("PlayerStatus instance not found. Immortality feature disabled.");
                        immortal = false; // Disable immortality if PlayerStatus instance is missing
                    }
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerStatus instance: " + ex.Message);
                    immortal = false; // Disable immortality if an error occurs
                }
            }

            // Noclip
            if (noclip)
            {
                GameObject playerObject = GameObject.Find("Player");
                if (playerObject != null)
                {
                    try
                    {
                        // Enable Noclip mode
                        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.detectCollisions = false;
                        }
                        else
                        {
                            Debug.LogError("Rigidbody component not found on player object.");
                        }
                    }
                    catch (NullReferenceException ex)
                    {
                        Debug.LogError("Error accessing Rigidbody component: " + ex.Message);
                        noclip = false; // Disable noclip if an error occurs
                    }
                }
                else
                {
                    Debug.LogError("Player object not found.");
                    noclip = false; // Disable noclip if player object is not found
                }
            }
            if (!noclip)
            {
                GameObject playerObject = GameObject.Find("Player");
                if (playerObject != null)
                {
                    try
                    {
                        // Disable Noclip mode
                        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.detectCollisions = true;
                        }
                        else
                        {
                            Debug.LogError("Rigidbody component not found on player object.");
                        }
                    }
                    catch (NullReferenceException ex)
                    {
                        Debug.LogError("Error accessing Rigidbody component: " + ex.Message);
                        noclip = false; // Disable noclip if an error occurs
                    }
                }
            }

            // Icy Floor
            if (slidSlides)
            {
                try
                {
                    PlayerMovement.Instance.grounded = false;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerMovement instance: " + ex.Message);
                    slidSlides = false; // Disable icy floor if an error occurs
                }
            }

            // Infinite Jumps
            if (infiniteJumps)
            {
                try
                {
                    PlayerStatus.Instance.stamina = 100;
                    PlayerMovement.Instance.grounded = true;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerMovement or PlayerStatus instance: " + ex.Message);
                    infiniteJumps = false; // Disable infinite jumps if an error occurs
                }
            }

            // Fly
            if (fly)
            {
                try
                {
                    // Set up the Rigid Body
                    GameObject playerObject = GameObject.Find("Player");
                    Transform orientation = playerObject.transform.Find("Orientation");
                    Rigidbody rb = playerObject.GetComponent<Rigidbody>();

                    // Calculate fly direction based on input
                    Vector3 flyDirection = Vector3.zero;

                    if (Input.GetKey(KeyCode.Space)) // Fly up
                    {
                        flyDirection += orientation.up;
                    }
                    else if (Input.GetKey(KeyCode.LeftControl)) // Fly down
                    {
                        flyDirection -= orientation.up;
                    }

                    if (Input.GetKey(KeyCode.W)) // Move forward
                    {
                        flyDirection += orientation.forward;
                    }
                    if (Input.GetKey(KeyCode.S)) // Move backward
                    {
                        flyDirection -= orientation.forward;
                    }

                    if (Input.GetKey(KeyCode.D)) // Move right
                    {
                        flyDirection += orientation.right;
                    }
                    if (Input.GetKey(KeyCode.A)) // Move left
                    {
                        flyDirection -= orientation.right;
                    }
                    // Faster Fly
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (!flySpeedBoost)
                        {
                            flySpeedBoost = true;
                            flySpeed = 60f;
                        }
                        PlayerStatus.Instance.stamina = 100;
                        PlayerStatus.Instance.hunger = 100;
                    }
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        if (flySpeedBoost)
                        {
                            flySpeedBoost = false;
                            flySpeed = 40f;
                        }
                    }

                    // Normalize the fly direction
                    if (flyDirection.magnitude > 1f)
                    {
                        flyDirection.Normalize();
                    }

                    // Apply velocity to simulate flying movement
                    rb.velocity = flyDirection * flySpeed;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing Player object: " + ex.Message);
                    fly = false; // Disable fly if an error occurs
                }
            }

            if (moonGravity)
            {
                try
                {
                    PlayerMovement.Instance.extraGravity = 0f;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerMovement instance: " + ex.Message);
                    moonGravity = false; // Disable infinite jumps if an error occurs
                }
            }

            if (jupiterGravity)
            {
                try
                {
                    PlayerMovement.Instance.extraGravity = 500f;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerMovement instance: " + ex.Message);
                    jupiterGravity = false; // Disable infinite jumps if an error occurs
                }
            }

            if (zeroGravity)
            {
                try
                {
                    PlayerMovement.Instance.extraGravity = -20f;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerMovement instance: " + ex.Message);
                    zeroGravity = false; // Disable infinite jumps if an error occurs
                }
            }

            if (sunGravity)
            {
                try
                {
                    PlayerMovement.Instance.extraGravity = 5000f;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerMovement instance: " + ex.Message);
                    sunGravity = false; // Disable infinite jumps if an error occurs
                }
            }

            if (!moonGravity & !jupiterGravity & !zeroGravity & !sunGravity)
            {
                try
                {
                    PlayerMovement.Instance.extraGravity = 50f;
                }
                catch (NullReferenceException)
                { }
            }

            if (neverHungry)
            {
                try
                {
                    PlayerStatus.Instance.hunger = 100;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerStatus instance: " + ex.Message);
                    neverHungry = false; // Disable immortality if an error occurs
                }
            }

            if (neverTired)
            {
                try
                {
                    PlayerStatus.Instance.stamina = 100;
                }
                catch (NullReferenceException ex)
                {
                    Debug.LogError("Error accessing PlayerStatus instance: " + ex.Message);
                    neverTired = false; // Disable immortality if an error occurs
                }
            }
            if (spamPlayers)
            {
                try
                {
                    lastPlayerId++;

                    Vector3 addPlayerPostion = new Vector3(GameObject.Find("Player").transform.position.x, GameObject.Find("Player").transform.position.y, GameObject.Find("Player").transform.position.z);

                    UnityEngine.Color color = Mods.GetRandomColor();

                    GameManager.instance.SpawnPlayer(lastPlayerId, chatMessage, Mods.GetRandomColor(), addPlayerPostion, Mods.GetRandomNumber(0, 100));
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error accessing GameManager instance: " + ex.Message);
                    spamPlayers = false; // Disable immortality if an error occurs
                }
            }
        }
    }
}
