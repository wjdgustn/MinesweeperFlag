using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MinesweeperFlag.MainPatch {
    // public class Text : MonoBehaviour {
    //     public static string Content = "Wa sans!";
    //     
    //     void OnGUI() {
    //         GUIStyle style = new GUIStyle();
    //         style.fontSize = (int) 50.0f;
    //         style.font = RDString.GetFontDataForLanguage(RDString.language).font;
    //         style.normal.textColor = Color.white;
    //
    //         GUI.Label(new Rect(10, 40, Screen.width, Screen.height), Content, style);
    //     }
    // }
    
    [HarmonyPatch(typeof(scrController), "ValidInputWasTriggered")]

    internal static class Test {
        private static bool Prefix() {
            if (SceneManager.GetActiveScene().name != "scnMinesweeper") return true;

            if (Input.GetMouseButtonDown(1)) {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var hit = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

                foreach (var h in hit) {
                    if (h.collider.gameObject.name.StartsWith("Floor")) {
                        var gameObject = h.collider.gameObject;
                        var existFlag = gameObject.transform.Find("Flag");

                        if (existFlag != null) {
                            Object.Destroy(existFlag.gameObject);
                            break;
                        }
                        
                        var flagObject = new GameObject("Flag");
                        flagObject.transform.SetParent(gameObject.transform);
                        flagObject.transform.localPosition = new Vector3(0, 0, 0);
                        flagObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        flagObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
                        flagObject.AddComponent<SpriteRenderer>();
                        flagObject.GetComponent<SpriteRenderer>().sprite = Main.FlagIcon.ToSprite();
                    }
                }

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(scnMinesweeper), "ProcessTile")]

    internal static class RemoveSelectFloorFlag {
        private static void Postfix(int index, scnMinesweeper __instance) {
            var floors = (scrFloor[]) AccessTools.Field(typeof(scnMinesweeper), "floors").GetValue(__instance);
            var floor = floors[index];
            var existFlag = floor.transform.Find("Flag");

            if (existFlag != null) Object.Destroy(existFlag.gameObject);
        }
    }

    [HarmonyPatch(typeof(scnMinesweeper), "Update")]

    internal static class IgnoreFlagTile {
        private static bool Prefix(scnMinesweeper __instance) {
            var readySetGo = (bool) AccessTools.Field(typeof(scnMinesweeper), "readySetGo").GetValue(__instance);
            if (readySetGo) return true;

            var selectedFloor = (int) AccessTools.Field(typeof(scnMinesweeper), "selectedFloor").GetValue(__instance);
            var floors = (scrFloor[]) AccessTools.Field(typeof(scnMinesweeper), "floors").GetValue(__instance);
            var floor = floors[selectedFloor];
            var existFlag = floor.transform.Find("Flag");

            // Text.Content = $"selectedFloor: {selectedFloor.ToString()}";

            if (existFlag != null && !Main.planetMoved) return false;

            Main.planetMoved = false;

            return true;
        }
    }

    [HarmonyPatch(typeof(scrPlanet), "SwitchChosen")]

    internal static class UpdateOnPlanetChange {
        private static void Postfix() {
            Main.planetMoved = true;
        }
    }
}