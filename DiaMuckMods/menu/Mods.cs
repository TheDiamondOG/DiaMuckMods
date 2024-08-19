using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Steamworks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static Mob;

namespace DiaMuckMods.menu
{
    internal class Mods : MonoBehaviour
    {
        public static int MobIdsCool = 0;
        public static int objectId = 9999;
        private static float jetpackVolocity;
        private static float jetpackVolocityMax = 100f;
        private static float jetpackVolocitySpeed = 2.5f;

        public static void SizeChanger(string objectName, Vector3 size)
        {
            GameObject boatObject = GameObject.Find(objectName);

            // Access the boat's transform component
            Transform boatTransform = boatObject.transform;

            // Resize the boat
            boatTransform.localScale = size;
        }


        public static void SpawnMob(string mobName, Vector3 position, float multiplyer=1f)
        {
            GameObject playerObject = GameObject.Find("Player");

            if (playerObject == null)
            {
                Debug.LogError("Player object not found.");
                return;
            }

            Transform playerTransform = playerObject.transform;

            if (position == null)
            {
                position = new Vector3(playerTransform.position.x, playerTransform.position.y + 5f, playerTransform.position.z);
            }

            // Use ScriptableObject.CreateInstance to create a MobType instance
            MobType mobType = ScriptableObject.CreateInstance<MobType>();

            int mobID = 0;

            // Find the correct MobType by name
            foreach (MobType mober in MobSpawner.Instance.allMobs)
            {
                if (mober.name.ToLower() == mobName.ToLower())
                {
                    mobID = mober.id;
                    break;
                }
            }

            MobIdsCool++;

            Vector3 mobSpawnSection = new Vector3(playerTransform.position.x, playerTransform.position.y + 5f, playerTransform.position.z);

            // Log the intended spawn position
            Debug.Log($"Attempting to spawn mob at position: {mobSpawnSection}");

            // Check if the position is close enough to the NavMesh
            if (!NavMesh.SamplePosition(mobSpawnSection, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                Debug.LogWarning("Spawn position is not close enough to the NavMesh. Adjusting position.");
                mobSpawnSection = hit.position;
                Debug.Log($"Adjusted spawn position to: {mobSpawnSection}");
            }

            // Spawn the mob
            MobSpawner.Instance.ServerSpawnNewMob(MobManager.Instance.GetNextId(), mobID, position, multiplyer, multiplyer*1.5f, (BossType)0, -1);
        }
        public static void SpawnGuardianMob(int guardianType, Vector3 position, float multiplyer = 1f)
        {
            string mobName = "Guardian";

            GameObject playerObject = GameObject.Find("Player");

            if (playerObject == null)
            {
                Debug.LogError("Player object not found.");
                return;
            }

            Transform playerTransform = playerObject.transform;

            if (position == null)
            {
                position = new Vector3(playerTransform.position.x, playerTransform.position.y + 5f, playerTransform.position.z);
            }

            // Use ScriptableObject.CreateInstance to create a MobType instance
            MobType mobType = ScriptableObject.CreateInstance<MobType>();

            int mobID = 0;

            // Find the correct MobType by name
            foreach (MobType mober in MobSpawner.Instance.allMobs)
            {
                if (mober.name.ToLower() == mobName.ToLower())
                {
                    mobID = mober.id;
                    break;
                }
            }

            MobIdsCool++;

            Vector3 mobSpawnSection = new Vector3(playerTransform.position.x, playerTransform.position.y + 5f, playerTransform.position.z);

            // Log the intended spawn position
            Debug.Log($"Attempting to spawn mob at position: {mobSpawnSection}");

            // Check if the position is close enough to the NavMesh
            if (!NavMesh.SamplePosition(mobSpawnSection, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                Debug.LogWarning("Spawn position is not close enough to the NavMesh. Adjusting position.");
                mobSpawnSection = hit.position;
                Debug.Log($"Adjusted spawn position to: {mobSpawnSection}");
            }

            // Spawn the mob
            MobSpawner.Instance.SpawnMob(position, mobID, MobIdsCool, 1f, 10f);
            MobSpawner.Instance.ServerSpawnNewMob(MobManager.Instance.GetNextId(), mobID, position, multiplyer, multiplyer * 1.5f, (BossType)0, guardianType);
        }
        public static void GiveItem(int ID, int amount)
        {
            try
            {
                objectId++;
                Debug.Log("Added one to objectid");

                //ItemManager.Instance.DropItemAtPosition(ID, amount, position, objectId);
                InventoryItem val = ItemManager.Instance.allItems[ID];
                val.amount = amount;
                InventoryUI.Instance.AddItemToInventory(val);
                Debug.Log("Force dropping item");
                //ItemManager.Instance.PickupItem(objectId);
                //Debug.Log("Force picked item");
            }
            catch (Exception e)
            {
                Debug.Log("Failed to give item: " + e);
                GiveItem(ID, amount);
            }
        }
        public static Color GetRandomColor()
        {
            System.Random random = new System.Random();
            float r = random.Next(256) / 255f; // Random value between 0 and 255, then normalized to 0-1
            float g = random.Next(256) / 255f;
            float b = random.Next(256) / 255f;

            return new Color(r, g, b);
        }

        public static Vector3 GetRandomVector3(float minRange, float maxRange)
        {
            System.Random random = new System.Random();
            float x = (float)(random.NextDouble() * (maxRange - minRange) + minRange);
            float y = (float)(random.NextDouble() * (maxRange - minRange) + minRange);
            float z = (float)(random.NextDouble() * (maxRange - minRange) + minRange);

            return new Vector3(x, y, z);
        }

        public static int GetRandomNumber(int minRange, int maxRange)
        {
            System.Random random = new System.Random();
            int x = random.Next(minRange, maxRange + 1);

            return x;
        }

        public static void Revive()
        {
            PlayerManager playerManager = GameManager.players[LocalClient.instance.myId];
            if (playerManager.dead && GameManager.state == GameManager.GameState.GameOver)
            {
                ClientSend.RevivePlayer(playerManager.id);
                ServerSend.RevivePlayer(playerManager.id, playerManager.id, true, 0);
                GameManager.instance.gameoverUi.SetActive(false);
                GameManager.state = GameManager.GameState.Playing;
                PlayerStatus.Instance.Respawn();
            }
        }

        public static void JetPack()
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
                if (jetpackVolocity < jetpackVolocityMax)
                {
                    jetpackVolocity += jetpackVolocitySpeed;
                }
                // Apply velocity to simulate flying movement
                rb.AddForce(flyDirection * jetpackVolocity);
            }
            if (!Input.GetKey(KeyCode.Space))
            {
                if (jetpackVolocity > 0)
                {
                    jetpackVolocity -= 1f;
                }
            }
        }


    }
}
