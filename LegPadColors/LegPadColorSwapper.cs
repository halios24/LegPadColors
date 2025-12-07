using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using AYellowpaper.SerializedCollections;

namespace LegPadColors;

public static class LegPadColorSwapper
{
    private static Color RedTeamLegPadColor => new Color(
        Plugin.modSettings.RedTeamPadRedValue,
        Plugin.modSettings.RedTeamPadGreenValue,
        Plugin.modSettings.RedTeamPadBlueValue
    );
    
    private static Color BlueTeamLegPadColor => new Color(
        Plugin.modSettings.BlueTeamPadRedValue,
        Plugin.modSettings.BlueTeamPadGreenValue,
        Plugin.modSettings.BlueTeamPadBlueValue
    );
    [HarmonyPatch(typeof(PlayerBodyV2), nameof(PlayerBodyV2.UpdateMesh))]
    public static class PlayerBodyV2UpdateMeshPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerBodyV2 __instance)
        {
            try
            {
                if (__instance == null || __instance.PlayerMesh == null || __instance.Player == null)
                {
                    Plugin.Log("LegPadColorSwapper: PlayerBodyV2, PlayerMesh, or Player instance is null in UpdateMesh Postfix. Skipping.");
                    return;
                }

                // Get references to the left and right leg pads
                PlayerLegPad leftLegPad = __instance.PlayerMesh.PlayerLegPadLeft;
                PlayerLegPad rightLegPad = __instance.PlayerMesh.PlayerLegPadRight;
                
                Color targetColor;
                if (__instance.Player.Role.Value == PlayerRole.Goalie) // Only apply custom color to goalies
                {
                    if (__instance.Player.Team.Value == PlayerTeam.Blue)
                    {
                        targetColor = BlueTeamLegPadColor;
                        Plugin.Log($"LegPadColorSwapper: Applying Blue Team Leg Pad color for {__instance.Player.Username.Value}");
                    }
                    else if (__instance.Player.Team.Value == PlayerTeam.Red)
                    {
                        targetColor = RedTeamLegPadColor;
                        Plugin.Log($"LegPadColorSwapper: Applying Red Team Leg Pad color for {__instance.Player.Username.Value}");
                    }
                    else
                    {
                        Plugin.Log($"LegPadColorSwapper: Player {__instance.Player.Username.Value} is a goalie but on unhandled team {__instance.Player.Team.Value}. Skipping custom pad color.");
                        return;
                    }
                }
                else
                {
                    // If the player is not a goalie, skip applying leg pad colors
                    Plugin.Log($"LegPadColorSwapper: Player {__instance.Player.Username.Value} is not a goalie ({__instance.Player.Role.Value}). Skipping leg pad color change.");
                    return;
                }
               
                // Apply color to the left leg pad
                if (leftLegPad != null)
                {
                    ApplyColorToLegPad(leftLegPad, targetColor, "Left");
                }
                else
                {
                    Plugin.Log("LegPadColorSwapper: Left PlayerLegPad is null. Cannot apply color.");
                }

                // Apply color to the right leg pad
                if (rightLegPad != null)
                {
                    ApplyColorToLegPad(rightLegPad, targetColor, "Right");
                }
                else
                {
                    Plugin.Log("LegPadColorSwapper: Right PlayerLegPad is null. Cannot apply color.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogError($"LegPadColorSwapper: Error in PlayerBodyV2.UpdateMesh Postfix: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }
        
        private static void ApplyColorToLegPad(PlayerLegPad legPad, Color color, string side)
        {
            MeshRenderer meshRenderer = legPad.gameObject.GetComponentInChildren<MeshRenderer>(true);

            if (meshRenderer == null)
            {
                Plugin.LogError($"LegPadColorSwapper: No MeshRenderer found on {side} Leg Pad ({legPad.gameObject.name}) or its children. Cannot apply color.");
                return;
            }

            if (meshRenderer.material != null && meshRenderer.material.name.Contains("Leg Pad", StringComparison.OrdinalIgnoreCase))
            {
                // Set colors
                meshRenderer.material.SetColor("_Color", color);
                meshRenderer.material.SetColor("_BaseColor", color);

                Plugin.Log($"LegPadColorSwapper: Applied color to {side} Leg Pad material ({meshRenderer.material.name}) on object {legPad.gameObject.name}. Color: {color}");
            }
            else
            {
                Plugin.Log($"LegPadColorSwapper: MeshRenderer found on {side} Leg Pad's child ({meshRenderer.gameObject.name}), but material name '{meshRenderer.material?.name ?? "NULL"}' is not 'Leg Pad'. Skipping color change.");
            }
        }
    }
}
    