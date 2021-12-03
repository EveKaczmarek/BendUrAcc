using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using MessagePack;
using ParadoxNotion.Serialization;

using BepInEx.Logging;
using HarmonyLib;

using ExtensibleSaveFormat;

using KKAPI;
using KKAPI.Chara;
using JetPack;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal static BendUrAccController GetController(ChaControl _chaCtrl) => _chaCtrl?.gameObject?.GetComponent<BendUrAccController>();
		internal static CharaCustomFunctionController GetBoneController(ChaControl _chaCtrl) => _chaCtrl?.gameObject?.GetComponent<KKABMX.Core.BoneController>();

		public partial class BendUrAccController : CharaCustomFunctionController
		{
			internal int _currentCoordinateIndex => ChaControl.fileStatus.coordinateType;

			internal bool _duringLoadChange = false;

			internal List<BendModifier> BendModifierList = new List<BendModifier>();
			internal List<BendModifier> BendModifierCache = new List<BendModifier>();

			protected override void OnCardBeingSaved(GameMode currentGameMode)
			{
				if (BendModifierList?.Count == 0)
				{
					SetExtendedData(null);
					return;
				}

				PluginData _pluginData = new PluginData();
				_pluginData.data.Add("BendModifierList", MessagePackSerializer.Serialize(BendModifierList));
				_pluginData.version = ExtDataVer;
				SetExtendedData(_pluginData);
			}

			protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
			{
				List<BendModifier> _data = ListCoordinateModifier().JsonClone<List<BendModifier>>();
				if (_data?.Count == 0)
				{
					SetCoordinateExtendedData(coordinate, null);
					return;
				}
				_data.ForEach(x => x.Coordinate = -1);

				PluginData _pluginData = new PluginData();
				_pluginData.data.Add("BendModifierList", MessagePackSerializer.Serialize(_data));
				_pluginData.version = ExtDataVer;
				SetCoordinateExtendedData(coordinate, _pluginData);
			}

			protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
			{
				_duringLoadChange = true;

				BendModifierList.RemoveAll(x => x.Coordinate == _currentCoordinateIndex);
				PluginData _pluginData = GetCoordinateExtendedData(coordinate);
				if (_pluginData != null)
				{
					if (_pluginData.version > ExtDataVer)
						_logger.Log(LogLevel.Error | LogLevel.Message, $"[BendUrAcc] ExtendedData.version: {_pluginData.version} is newer than your plugin");
					else
					{
						if (_pluginData.data.TryGetValue("BendModifierList", out object _loadedBendModifierList) && _loadedBendModifierList != null)
						{
							List<BendModifier> _tempBendModifierList = MessagePackSerializer.Deserialize<List<BendModifier>>((byte[]) _loadedBendModifierList);
							if (_tempBendModifierList?.Count > 0)
							{
								_tempBendModifierList.ForEach(x => x.Coordinate = _currentCoordinateIndex);
								BendModifierList.AddRange(_tempBendModifierList);
							}
						}
					}
				}
				RefreshCache();
				StartCoroutine(OnCoordinateBeingLoadedCoroutine());
				base.OnCoordinateBeingLoaded(coordinate);
			}

			private IEnumerator OnCoordinateBeingLoadedCoroutine()
			{
				yield return JetPack.Toolbox.WaitForEndOfFrame;
				yield return JetPack.Toolbox.WaitForEndOfFrame;
				_duringLoadChange = false;
			}

			protected override void OnReload(GameMode currentGameMode)
			{
				_duringLoadChange = true;

				BendModifierList.Clear();
				PluginData _pluginData = GetExtendedData();
				if (_pluginData != null)
				{
					if (_pluginData.version > ExtDataVer)
						_logger.Log(LogLevel.Error | LogLevel.Message, $"[BendUrAcc] ExtendedData.version: {_pluginData.version} is newer than your plugin");
					else
					{
						if (_pluginData.data.TryGetValue("BendModifierList", out object _loadedBendModifierList) && _loadedBendModifierList != null)
						{
							List<BendModifier> _tempBendModifierList = MessagePackSerializer.Deserialize<List<BendModifier>>((byte[]) _loadedBendModifierList);
							if (_tempBendModifierList?.Count > 0)
								BendModifierList.AddRange(_tempBendModifierList);
						}
					}
				}
				RefreshCache();
				StartCoroutine(OnCoordinateBeingLoadedCoroutine());
				base.OnReload(currentGameMode);
			}

			internal void RefreshCache()
			{
				BendModifierCache.Clear();
				BendModifierCache = ListCoordinateModifier(_currentCoordinateIndex);
			}

			internal void AccessoriesCopiedHandler(int _srcCoordinateIndex, int _dstCoordinateIndex, List<int> _copiedSlotIndexes)
			{
				foreach (int _slotIndex in _copiedSlotIndexes)
					CloneModifier(_slotIndex, _slotIndex, _srcCoordinateIndex, _dstCoordinateIndex);

				if (_dstCoordinateIndex == _currentCoordinateIndex)
				{
					RefreshCache();
					StartCoroutine(ApplyBendModifierListHack("AccessoriesCopiedHandler"));
				}
			}

			internal void AccessoryTransferredHandler(int _srcSlotIndex, int _dstSlotIndex)
			{
				CloneModifier(_srcSlotIndex, _dstSlotIndex, _currentCoordinateIndex);

				RefreshCache();
				StartCoroutine(ApplyBendModifierListHack("AccessoryTransferredHandler"));
			}

			internal List<BendModifier> ListCoordinateModifier() => ListCoordinateModifier(_currentCoordinateIndex);
			internal List<BendModifier> ListCoordinateModifier(int _coordinateIndex) => BendModifierList.Where(x => x.Coordinate == _coordinateIndex).ToList();

			internal List<BendModifier> ListSlotModifier(int _slotIndex) => ListSlotModifier(_currentCoordinateIndex, _slotIndex);
			internal List<BendModifier> ListSlotModifier(int _coordinateIndex, int _slotIndex) => BendModifierList.Where(x => x.Coordinate == _coordinateIndex && x.Slot == _slotIndex).ToList();

			internal BendModifier GetModifier(int _slotIndex, string _nodeName) => GetModifier(_currentCoordinateIndex, _slotIndex, _nodeName);
			internal BendModifier GetModifier(int _coordinateIndex, int _slotIndex, string _nodeName) => BendModifierList.FirstOrDefault(x => x.Coordinate == _coordinateIndex && x.Slot == _slotIndex && x.Node == _nodeName);

			internal IEnumerator ApplyBendModifierListHack(string _caller)
			{
				if (_duringLoadChange)
					yield break;

				yield return JetPack.Toolbox.WaitForEndOfFrame;
				yield return JetPack.Toolbox.WaitForEndOfFrame;

				ApplyBendModifierList(_caller);
			}

			internal void ApplyBendModifierList(string _caller)
			{
				if (_duringLoadChange) return;
				AccGotHighRemoveEffect();
				RefreshCache();

				if (BendModifierCache?.Count == 0) return;

				StartCoroutine(ApplyBendModifierListCoroutine());
			}

			internal void CheckNoShake(ChaFileAccessory.PartsInfo _part, GameObject _ca_slot)
			{
				bool _noShake = _part.noShake;
				List<DynamicBone> _cmps = _ca_slot.GetComponent<ComponentLookupTable>().Components<DynamicBone>().Where(x => x.m_Root != null).ToList();
				if (_cmps.Count == 0) return;

				foreach (DynamicBone _cmp in _cmps)
					_cmp.enabled = !_noShake;
			}

			internal IEnumerator ApplyBendModifierListCoroutine()
			{
				if (_duringLoadChange)
					yield break;

				yield return JetPack.Toolbox.WaitForEndOfFrame;
				yield return JetPack.Toolbox.WaitForEndOfFrame;

				if (BendModifierCache.Any(x => x.SlotType == SlotType.Accessory))
				{
					List<ChaFileAccessory.PartsInfo> _parts = JetPack.Accessory.ListPartsInfo(ChaControl, _currentCoordinateIndex);
					List<GameObject> _objAccessories = JetPack.Accessory.ListObjAccessory(ChaControl);
					HashSet<int> _slots = new HashSet<int>(BendModifierCache.Select(x => x.Slot).OrderBy(x => x));
					foreach (int _slotIndex in _slots)
					{
						ChaFileAccessory.PartsInfo _part = _parts.ElementAtOrDefault(_slotIndex);
						if (_part.type == 120) continue;

						GameObject _ca_slot = _objAccessories.FirstOrDefault(x => x.name == $"ca_slot{_slotIndex:00}");
						if (_ca_slot == null) continue;

						CheckNoShake(_part, _ca_slot);

						foreach (BendModifier _modifier in BendModifierCache.Where(x => x.SlotType == SlotType.Accessory && x.Slot == _slotIndex).ToList())
						{
							Transform _node = _ca_slot.transform.Find(_modifier.NodePath);
							if (_node == null)
							{
								DebugMsg(LogLevel.Warning, $"cannot find GameObject {_modifier.Node} in ca_slot{_slotIndex:00}");
								continue;
							}
							_node.localPosition = _modifier.Position.JsonClone<Vector3>();
							_node.localEulerAngles = _modifier.Rotation.JsonClone<Vector3>();
							_node.localScale = _modifier.Scale.JsonClone<Vector3>();
						}
					}
				}

				Traverse.Create(GetBoneController(ChaControl)).Property("NeedsBaselineUpdate").SetValue(true);
			}

			internal void CloneModifier(int _srcSlotIndex, int _dstSlotIndex) => CloneModifier(_srcSlotIndex, _dstSlotIndex, _currentCoordinateIndex, _currentCoordinateIndex);
			internal void CloneModifier(int _srcSlotIndex, int _dstSlotIndex, int _coordinateIndex) => CloneModifier(_srcSlotIndex, _dstSlotIndex, _coordinateIndex, _coordinateIndex);
			internal void CloneModifier(int _srcSlotIndex, int _dstSlotIndex, int _srcCoordinateIndex, int _dstCoordinateIndex)
			{
				BendModifierList.RemoveAll(x => x.Coordinate == _dstCoordinateIndex && x.Slot == _dstSlotIndex);
				List<BendModifier> _modifiers = ListSlotModifier(_srcCoordinateIndex, _srcSlotIndex).JsonClone<List<BendModifier>>();
				if (_modifiers?.Count == 0) return;
				_modifiers.ForEach(x => x.Coordinate = _dstCoordinateIndex);
				_modifiers.ForEach(x => x.Slot = _dstSlotIndex);
				BendModifierList.AddRange(_modifiers);
			}

			internal void RemoveModifier(int _slotIndex, string _nodeName) => RemoveModifier(_currentCoordinateIndex, _slotIndex, _nodeName);
			internal void RemoveModifier(int _coordinateIndex, int _slotIndex, string _nodeName)
			{
				BendModifierList.RemoveAll(x => x.Coordinate == _coordinateIndex && x.Slot == _slotIndex && x.Node == _nodeName);
			}

			internal void RemoveSlotModifier(int _slotIndex) => RemoveSlotModifier(_currentCoordinateIndex, _slotIndex);
			internal void RemoveSlotModifier(int _coordinateIndex, int _slotIndex)
			{
				BendModifierList.RemoveAll(x => x.Coordinate == _coordinateIndex && x.Slot == _slotIndex);
			}

			internal void RemoveCoordinateModifier()
			{
				BendModifierList.RemoveAll(x => x.Coordinate == _currentCoordinateIndex);
			}
		}
	}
}
