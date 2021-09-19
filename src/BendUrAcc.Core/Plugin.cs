using System;
using System.Collections;

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
	public partial class BendUrAcc : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.BendUrAcc";
		public const string Name = "BendUrAcc";
		public const string Version = "1.0.4.0";

		internal static ConfigEntry<bool> _cfgDebugMode;

		internal static ConfigEntry<float> _cfgMakerWinX;
		internal static ConfigEntry<float> _cfgMakerWinY;
		internal static ConfigEntry<bool> _cfgMakerWinResScale;

		internal static ConfigEntry<float> _cfgPosIncValue;
		internal static ConfigEntry<float> _cfgRotIncValue;
		internal static ConfigEntry<float> _cfgSclIncValue;

		internal static ManualLogSource _logger;
		internal static BendUrAcc _instance;
		internal static Harmony _hooksInstance;

		internal static BendUrAccUI _charaConfigWindow;
		internal static MakerButton _accWinCtrlEnable;
		internal static Type BoneController;
		internal static Type ChaAccessoryClothes;

		private void Awake()
		{
			_logger = base.Logger;
			_instance = this;

			_cfgDebugMode = Config.Bind("Debug", "Debug Mode", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 20 }));
			_cfgMakerWinX = Config.Bind("Maker", "Config Window Startup X", 525f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 19 }));
			_cfgMakerWinX.SettingChanged += (_sender, _args) =>
			{
				if (_charaConfigWindow != null)
				{
					if (_charaConfigWindow._windowPos.x != _cfgMakerWinX.Value)
						_charaConfigWindow._windowPos.x = _cfgMakerWinX.Value;
				}
			};
			_cfgMakerWinY = Config.Bind("Maker", "Config Window Startup Y", 80f, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 18 }));
			_cfgMakerWinY.SettingChanged += (_sender, _args) =>
			{
				if (_charaConfigWindow != null)
				{
					if (_charaConfigWindow._windowPos.y != _cfgMakerWinY.Value)
						_charaConfigWindow._windowPos.y = _cfgMakerWinY.Value;
				}
			};
			_cfgMakerWinResScale = Config.Bind("Maker", "Config Window Resolution Adjust", false, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 17 }));
			_cfgMakerWinResScale.SettingChanged += (_sender, _args) =>
			{
				if (_charaConfigWindow == null) return;
				_charaConfigWindow.ChangeRes();
			};
			_cfgPosIncValue = Config.Bind("Config", "Position Increament", 0.001f, new ConfigDescription("Position increament/decrement initiial setting", new AcceptableValueList<float>(0.001f, 0.01f, 0.1f, 1f), new ConfigurationManagerAttributes { Order = 8 }));
			_cfgRotIncValue = Config.Bind("Config", "Rotation Increament", 1f, new ConfigDescription("Rotation increament/decrement initiial setting", new AcceptableValueList<float>(0.01f, 0.1f, 1f, 10f), new ConfigurationManagerAttributes { Order = 7 }));
			_cfgSclIncValue = Config.Bind("Config", "Scale Increament", 0.01f, new ConfigDescription("Scale increament/decrement initiial setting", new AcceptableValueList<float>(0.001f, 0.01f, 0.1f, 1f), new ConfigurationManagerAttributes { Order = 6 }));
		}

		private void Start()
		{
			CharacterApi.RegisterExtraBehaviour<BendUrAccController>(ExtDataKey);

			{
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("madevil.kk.MovUrAcc");
				if (_instance != null && !JetPack.Toolbox.PluginVersionCompare(_instance, "1.10.1.0"))
					_logger.LogError($"MovUrAcc 1.10.1.0 is required to work properly, version {_instance.Info.Metadata.Version} detected");
			}

			{
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("madevil.kk.ca");
				if (_instance != null && !JetPack.Toolbox.PluginVersionCompare(_instance, "1.7.0.0"))
					_logger.LogError($"Character Accessory 1.7.0.0 is required to work properly, version {_instance.Info.Metadata.Version} detected");
			}

			{
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("KKABMX.Core");
				BoneController = _instance.GetType().Assembly.GetType("KKABMX.Core.BoneController");
			}

			{
				BaseUnityPlugin _instance = JetPack.Toolbox.GetPluginInstance("com.deathweasel.bepinex.accessoryclothes");
				ChaAccessoryClothes = _instance.GetType().Assembly.GetType("KK_Plugins.ChaAccessoryClothes");
			}

			JetPack.Chara.OnChangeCoordinateType += (_sender, _args) =>
			{
				if (_args.State == "Prefix")
					_charaConfigWindow?.ResetAll();
				OnChangeCoordinateType(_args);
			};

			JetPack.MaterialEditor.OnDataApply += (_sender, _args) =>
			{
				if (_args.State != "Coroutine") return;

				BendUrAccController _pluginCtrl = GetController((_args.Controller as CharaCustomFunctionController).ChaControl);
				if (_pluginCtrl == null) return;

				//_pluginCtrl._duringLoadChange = false;
				_pluginCtrl.ApplyBendModifierList("OnDataApply");
			};

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

			MakerAPI.RegisterCustomSubCategories += (_sender, _args) =>
			{
				_charaConfigWindow = gameObject.AddComponent<BendUrAccUI>();
				_accWinCtrlEnable = MakerAPI.AddAccessoryWindowControl(new MakerButton("BendUrAcc", null, this));
				_accWinCtrlEnable.OnClick.AddListener(() => _charaConfigWindow.enabled = true);
			};

			MakerAPI.MakerFinishedLoading += (_sender, _args) =>
			{
				_hooksInstance = Harmony.CreateAndPatchAll(typeof(HooksMaker), GUID);

				if (JetPack.Game.HasDarkness)
				{
					_hooksInstance.Patch(Type.GetType("ChaControl, Assembly-CSharp").GetMethod("ChangeShakeAccessory", AccessTools.all, null, new[] { typeof(int) }, null), postfix: new HarmonyMethod(typeof(HooksMaker), nameof(HooksMaker.ChaControl_ChangeShakeAccessory_Postfix)));
				}
			};

			MakerAPI.MakerExiting += (_sender, _args) =>
			{
				Destroy(_charaConfigWindow);

				_hooksInstance.UnpatchAll(_hooksInstance.Id);
				_hooksInstance = null;
			};
		}

		internal static IEnumerator ToggleButtonVisibility()
		{
			yield return JetPack.Toolbox.WaitForEndOfFrame;
			yield return JetPack.Toolbox.WaitForEndOfFrame;

			ChaFileAccessory.PartsInfo _part = JetPack.Accessory.GetPartsInfo(CustomBase.Instance.chaCtrl, JetPack.CharaMaker.CurrentAccssoryIndex);
			_accWinCtrlEnable.Visible.OnNext(_part?.type != 120);
		}

		internal static void OnChangeCoordinateType(JetPack.Chara.ChangeCoordinateTypeEventArgs _args)
		{
			if (_args.State == "Prefix")
				OnChangeCoordinateType_Prefix(_args.ChaControl);
			else if (_args.State == "Postfix")
				OnChangeCoordinateType_Postfix(_args.ChaControl);
			else if (_args.State == "Coroutine")
				OnChangeCoordinateType_Coroutine(_args.ChaControl);
		}

		internal static void OnChangeCoordinateType_Prefix(ChaControl _chaCtrl)
		{
			BendUrAccController _pluginCtrl = GetController(_chaCtrl);
			if (_pluginCtrl == null) return;

			_pluginCtrl._duringLoadChange = true;
		}

		internal static void OnChangeCoordinateType_Postfix(ChaControl _chaCtrl)
		{
			BendUrAccController _pluginCtrl = GetController(_chaCtrl);
			if (_pluginCtrl == null) return;

			_pluginCtrl.RefreshCache();
		}

		internal static void OnChangeCoordinateType_Coroutine(ChaControl _chaCtrl)
		{
			BendUrAccController _pluginCtrl = GetController(_chaCtrl);
			if (_pluginCtrl == null) return;

			_pluginCtrl._duringLoadChange = false;
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