using MelonLoader;
using BTD_Mod_Helper;
using InstantDegree;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using System;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using Il2CppTMPro;
using MoreDegrees;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(InstantDegree.Main), ModHelperData.Name, ModHelperData.Version, "DepletedNova & GrahamKracker & Tanner")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace InstantDegree
{
    using static Int32;

    public class Main : BloonsTD6Mod
    {
        private static readonly ModSettingBool Enabled = new(true) { displayName = "Enabled" };
        // UI
        private static GameObject? _paragonButton;

        [HarmonyPatch(typeof(TSMThemeDefault), nameof(TSMThemeDefault.TowerInfoChanged))]
        // ReSharper disable once InconsistentNaming
        private static class TSMThemeDefault_TowerInfoChanged
        {
            private static TowerToSimulation? _selectedTower;

            [HarmonyPostfix]
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(TSMThemeDefault __instance, TowerToSimulation tower)
            {
                if (!tower.IsParagon) return;
                _selectedTower = tower;
                if (_paragonButton == null && Enabled)
                {
                    var container = __instance.transform.parent.parent.parent.FindChild("SelectedTowerOptions").FindChild("ParagonDetails").FindChild("ParagonInfo");
                    var obj = __instance.transform.FindChild("CloseButton").gameObject;
                    _paragonButton = Object.Instantiate(obj, container, true);
                    _paragonButton.transform.localPosition = new Vector3(0, 0, 0);
                    var rect = _paragonButton.GetComponent<RectTransform>();
                    rect.localPosition = new Vector3(-140, 10);
                    rect.sizeDelta = new Vector2(643, 235);
                    //Il2CppNinjaKiwi.Common.ResourceUtils.SpriteReference(ModContent.GetTextureGUID<Main>("DegreeButton"));
                    _paragonButton.GetComponent<Image>().SetSprite(ModContent.CreateSpriteReference(ModContent.GetTextureGUID<Main>("DegreeButton")));
                    _paragonButton.gameObject.GetComponent<Button>().SetOnClick(() =>
                    {
                        var paragonTower = _selectedTower.tower.GetTowerBehavior<ParagonTower>();
                        PopupScreen.instance.ShowSetNamePopup("Instant Degree", "Set Paragon degree:", new Action<string>(y =>
                        {
                            int x;
                            try
                            {
                                x = Parse(y);
                            }
                            catch (OverflowException)
                            {
                                x = MaxValue;
                            }
                            SetDegree(x, ref paragonTower);
                            paragonTower.CreateDegreeText();
                        }), "100");
                        PopupScreen.instance.ModifyField(tmp => { tmp.characterLimit = MaxValue; });
                        PopupScreen.instance.ModifyField(tmp => { tmp.characterValidation = TMP_InputField.CharacterValidation.Integer; });
                    });
                }
                else if (_paragonButton != null && !Enabled)
                {
                    _paragonButton.Destroy();
                    _paragonButton = null;
                }
            }
        }

        private static void SetDegree(int x, ref ParagonTower paragonTower)
        {
            var info = paragonTower.investmentInfo;
            var y = Math.Min(InGame.instance.GetGameModel().paragonDegreeDataModel.degreeCount, x);
            var z = Math.Max(1, y);

            if (z <= Game.instance.model.paragonDegreeDataModel.powerDegreeRequirements.Length)
            {
                info.totalInvestment = Game.instance.model.paragonDegreeDataModel.powerDegreeRequirements[z - 1];
                paragonTower.investmentInfo = info;
                paragonTower.UpdateDegree();
                return;
            }
            info.totalInvestment = CalculateInvestment(z);
            paragonTower.investmentInfo = info;
            paragonTower.UpdateDegree();
        }

        private static float CalculateInvestment(float degree)
        {
            var investment = 50f * (float)Math.Pow(degree, 3);
            investment += 5025f * (float)Math.Pow(degree, 2);
            investment += 168324f * degree;
            investment += 843000f;
            investment /= 590f;
            return investment;
        }
    }
}