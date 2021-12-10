using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ChaCustom;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Utilities;

namespace BendUrAcc
{
	[BepInDependency("madevil.JetPack", JetPack.Core.Version)]
	[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
	[BepInPlugin(GUID, Name, Version)]
	[BepInIncompatibility("KK_ClothesLoadOption")]
#if !DEBUG
	[BepInIncompatibility("com.jim60105.kk.studiocoordinateloadoption")]
	[BepInIncompatibility("com.jim60105.kk.coordinateloadoption")]
#endif
	public partial class BendUrAcc : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.BendUrAcc";
#if DEBUG
		public const string Name = "BendUrAcc (Debug Build)";
#else
		public const string Name = "BendUrAcc";
#endif
		public const string Version = "1.3.1.1";

		internal static ConfigEntry<bool> _cfgDebugMode;

		internal static ConfigEntry<float> _cfgMakerWinX;
		internal static ConfigEntry<float> _cfgMakerWinY;
		internal static ConfigEntry<bool> _cfgMakerWinResScale;
		internal static ConfigEntry<bool> _cfgDragPass;
		internal static ConfigEntry<float> _cfgPosIncValue;
		internal static ConfigEntry<float> _cfgRotIncValue;
		internal static ConfigEntry<float> _cfgSclIncValue;

		internal static ConfigEntry<Color> _cfgBonelyfanColor;

		internal static ManualLogSource _logger;
		internal static BendUrAcc _instance;
		internal static Harmony _hooksInstance;

		internal static BendUrAccUI _charaConfigWindow;
		internal static MakerButton _accWinCtrlEnable;
		internal static Type BoneController;
		internal static Type ChaAccessoryClothes;
		internal static Dictionary<string, Type> _types = new Dictionary<string, Type>();
		internal static string _boneInicatorName = "BendUrAcc_indicator";

		private void Awake()
		{
			_logger = base.Logger;
			_instance = this;
		}

		private void Start()
		{
#if KK
			if (JetPack.MoreAccessories.BuggyBootleg)
			{
#if DEBUG
				if (!JetPack.MoreAccessories.Installed)
				{
					_logger.LogError($"Backward compatibility in BuggyBootleg MoreAccessories is disabled");
					return;
				}
#else
				_logger.LogError($"Could not load {Name} {Version} because it is incompatible with MoreAccessories experimental build");
				return;
#endif
			}

			if (!JetPack.Game.HasDarkness)
			{
				_logger.LogError($"This plugin requires Darkness to run");
				return;
			}
#endif
			_cfgDebugMode = Config.Bind("Debug", "Debug Mode", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 20 }));
			_cfgDragPass = Config.Bind("Maker", "Drag Pass Mode", false, new ConfigDescription("Setting window will not block mouse dragging", null, new ConfigurationManagerAttributes { Order = 15, Browsable = !JetPack.CharaStudio.Running }));
			_cfgDragPass.SettingChanged += delegate
			{
				if (_charaConfigWindow != null)
				{
					if (_charaConfigWindow._passThrough != _cfgDragPass.Value)
						_charaConfigWindow._passThrough = _cfgDragPass.Value;
				}
			};

			_cfgMakerWinX = Config.Bind("Maker", "Config Window Startup X", 525f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 19 }));
			_cfgMakerWinX.SettingChanged += delegate
			{
				if (_charaConfigWindow != null)
				{
					if (_charaConfigWindow._windowPos.x != _cfgMakerWinX.Value)
						_charaConfigWindow._windowPos.x = _cfgMakerWinX.Value;
				}
			};
			_cfgMakerWinY = Config.Bind("Maker", "Config Window Startup Y", 80f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 18 }));
			_cfgMakerWinY.SettingChanged += delegate
			{
				if (_charaConfigWindow != null)
				{
					if (_charaConfigWindow._windowPos.y != _cfgMakerWinY.Value)
						_charaConfigWindow._windowPos.y = _cfgMakerWinY.Value;
				}
			};
			_cfgMakerWinResScale = Config.Bind("Maker", "Config Window Resolution Adjust", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 17 }));
			_cfgMakerWinResScale.SettingChanged += delegate
			{
				if (_charaConfigWindow != null)
					_charaConfigWindow.ChangeRes();
			};
			_cfgPosIncValue = Config.Bind("Config", "Position Increament", 0.001f, new ConfigDescription("Position increament/decrement initiial setting", new AcceptableValueList<float>(0.001f, 0.01f, 0.1f, 1f), new ConfigurationManagerAttributes { Order = 8 }));
			_cfgRotIncValue = Config.Bind("Config", "Rotation Increament", 1f, new ConfigDescription("Rotation increament/decrement initiial setting", new AcceptableValueList<float>(0.01f, 0.1f, 1f, 10f), new ConfigurationManagerAttributes { Order = 7 }));
			_cfgSclIncValue = Config.Bind("Config", "Scale Increament", 0.01f, new ConfigDescription("Scale increament/decrement initiial setting", new AcceptableValueList<float>(0.001f, 0.01f, 0.1f, 1f), new ConfigurationManagerAttributes { Order = 6 }));

			_cfgBonelyfanColor = Config.Bind("Maker", "Indicator Color", new Color(1, 0f, 0f, 1f), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 16, Browsable = !JetPack.CharaStudio.Running }));
			_cfgBonelyfanColor.SettingChanged += delegate
			{
				if (_assetSphere == null) return;
				_assetSphere.GetComponent<Renderer>().material.SetColor("_Color", _cfgBonelyfanColor.Value);
				if (_charaConfigWindow == null || _charaConfigWindow._boneInicator == null) return;
				_charaConfigWindow._boneInicator.GetComponent<Renderer>().material.SetColor("_Color", _cfgBonelyfanColor.Value);
			};

			{
				string _version = "1.10.3";
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("madevil.kk.MovUrAcc");
				if (_instance != null && !JetPack.Toolbox.PluginVersionCompare(_instance, _version))
				{
					_logger.LogError($"MovUrAcc {_version}+ is required to work properly, version {_instance.Info.Metadata.Version} detected");
					if (!JetPack.Game.ConsoleActive)
						_logger.LogMessage($"[{Name}] MovUrAcc {_version}+ is required to work properly, version {_instance.Info.Metadata.Version} detected");
				}
			}

			{
				string _version = "1.7.2";
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("madevil.kk.ca");
				if (_instance != null && !JetPack.Toolbox.PluginVersionCompare(_instance, _version))
				{
					_logger.LogError($"Character Accessory {_version}+ is required to work properly, version {_instance.Info.Metadata.Version} detected");
					if (!JetPack.Game.ConsoleActive)
						_logger.LogMessage($"[{Name}] Character Accessory {_version}+ is required to work properly, version {_instance.Info.Metadata.Version} detected");
				}
			}

			{
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("KKABMX.Core");
				BoneController = _instance.GetType().Assembly.GetType("KKABMX.Core.BoneController");
			}

			{
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("com.deathweasel.bepinex.accessoryclothes");
				ChaAccessoryClothes = _instance.GetType().Assembly.GetType("KK_Plugins.ChaAccessoryClothes");
			}

			{
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("madevil.kk.AccGotHigh");
				if (_instance != null)
					_types["AccGotHigh"] = _instance.GetType();
			}

			CharacterApi.RegisterExtraBehaviour<BendUrAccController>(ExtDataKey);

#if KKS
			InitCardImport();
#endif
			AccessoriesApi.AccessoryTransferred += (_sender, _args) => GetController(CustomBase.Instance.chaCtrl).AccessoryTransferredHandler(_args.SourceSlotIndex, _args.DestinationSlotIndex);
			AccessoriesApi.AccessoriesCopied += (_sender, _args) => GetController(CustomBase.Instance.chaCtrl).AccessoriesCopiedHandler((int) _args.CopySource, (int) _args.CopyDestination, _args.CopiedSlotIndexes.ToList());

			JetPack.Chara.OnChangeCoordinateType += (_sender, _args) => OnChangeCoordinateType(_args);

			JetPack.MaterialEditor.OnDataApply += (_sender, _args) => OnDataApply(_args);

			JetPack.CharaMaker.OnCvsNavMenuClick += (_sender, _args) =>
			{
				BendUrAccController _pluginCtrl = GetController(CustomBase.Instance.chaCtrl);

				if (_args.TopIndex == 4)
				{
					_charaConfigWindow._onAccTab = true;
					StartCoroutine(ToggleButtonVisibility());

					if (_args.SideToggle?.GetComponentInChildren<CvsAccessory>(true) == null)
					{
						_charaConfigWindow.enabled = false;
						return;
					}
				}
				else
				{
					_charaConfigWindow._onAccTab = false;
					_charaConfigWindow.enabled = false;
				}
			};

			MakerAPI.RegisterCustomSubCategories += delegate
			{
				_charaConfigWindow = gameObject.AddComponent<BendUrAccUI>();
				_accWinCtrlEnable = MakerAPI.AddAccessoryWindowControl(new MakerButton("BendUrAcc", null, this));
				_accWinCtrlEnable.OnClick.AddListener(() => _charaConfigWindow.enabled = true);
				_accWinCtrlEnable.GroupingID = "Madevil";
				_accWinCtrlEnable.Visible.OnNext(false);
			};

			MakerAPI.MakerFinishedLoading += delegate
			{
				_hooksInstance = Harmony.CreateAndPatchAll(typeof(HooksMaker), GUID);
			};

			MakerAPI.MakerExiting += delegate
			{
				Destroy(_charaConfigWindow);

				_hooksInstance.UnpatchAll(_hooksInstance.Id);
				_hooksInstance = null;
			};

			Init_Indicator();
		}

		internal static IEnumerator ToggleButtonVisibility()
		{
			yield return JetPack.Toolbox.WaitForEndOfFrame;
			yield return JetPack.Toolbox.WaitForEndOfFrame;

			if (JetPack.CharaMaker.CurrentAccssoryIndex < 0)
				_accWinCtrlEnable.Visible.OnNext(false);
			else
			{
				ChaFileAccessory.PartsInfo _part = JetPack.Accessory.GetPartsInfo(CustomBase.Instance.chaCtrl, JetPack.CharaMaker.CurrentAccssoryIndex);
				_accWinCtrlEnable.Visible.OnNext(_part != null && _part.type != 120);
			}
		}

		internal static void AccGotHighRemoveEffect()
		{
			if (!_types.ContainsKey("AccGotHigh"))
				return;
			Traverse.Create(_types["AccGotHigh"]).Method("RemoveEffect").GetValue();
		}

		internal static void DebugMsg(LogLevel _level, string _meg)
		{
			if (_cfgDebugMode.Value)
				_logger.Log(_level, _meg);
			else
				_logger.Log(LogLevel.Debug, _meg);
		}
	}
}