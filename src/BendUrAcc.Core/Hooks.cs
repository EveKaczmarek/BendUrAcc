using System.Collections;
using System.Linq;

using UnityEngine;
using ChaCustom;

using HarmonyLib;

using JetPack;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal static void Refresh() // for only position changed case
		{
			_instance.StartCoroutine(ToggleButtonVisibility());

			if (_charaConfigWindow == null) return;
			_charaConfigWindow._needRefreshSlotInfo = true;
		}

		internal static class HooksMaker
		{
			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.FuncUpdateAcsPosAdd), new[] { typeof(int), typeof(int), typeof(bool), typeof(float) })]
			internal static void CvsAccessory_FuncUpdateAcsPosAdd_Prefix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.FuncUpdateAcsRotAdd), new[] { typeof(int), typeof(int), typeof(bool), typeof(float) })]
			internal static void CvsAccessory_FuncUpdateAcsRotAdd_Prefix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.FuncUpdateAcsSclAdd), new[] { typeof(int), typeof(int), typeof(bool), typeof(float) })]
			internal static void CvsAccessory_FuncUpdateAcsSclAdd_Prefix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryType), new[] { typeof(int) })]
			private static void CvsAccessory_UpdateSelectAccessoryType_Postfix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryParent), new[] { typeof(int) })]
			private static void CvsAccessory_UpdateSelectAccessoryParent_Postfix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessory), nameof(CvsAccessory.UpdateSelectAccessoryKind), new[] { typeof(string), typeof(Sprite), typeof(int) })]
			private static void CvsAccessory_UpdateSelectAccessoryKind_Postfix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessoryChange), nameof(CvsAccessoryChange.CopyAcs))]
			private static void CvsAccessoryChange_CopyAcs_Postfix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessoryChange), nameof(CvsAccessoryChange.CopyAcsCorrect), new[] { typeof(int) })]
			private static void CvsAccessoryChange_CopyAcsCorrect_Postfix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessoryChange), nameof(CvsAccessoryChange.CopyAcsCorrectRevLR), new[] { typeof(int) })]
			private static void CvsAccessoryChange_CopyAcsCorrectRevLR_Postfix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessoryChange), nameof(CvsAccessoryChange.CopyAcsCorrectRevUD), new[] { typeof(int) })]
			private static void CvsAccessoryChange_CopyAcsCorrectRevUD_Postfix() => Refresh();

			[HarmonyPrefix, HarmonyPatch(typeof(CvsAccessoryCopy), nameof(CvsAccessoryCopy.CopyAcs))]
			private static void CvsAccessoryChange_CopyAcs_Postfix(CvsAccessoryCopy __instance)
			{
				if (CustomBase.Instance.chaCtrl.fileStatus.coordinateType == __instance.ddCoordeType[0].value)
					_charaConfigWindow.ResetAll();
			}

			[HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateTypeAndReload), new[] { typeof(bool) })]
			private static void ChaControl_ChangeCoordinateTypeAndReload_Postfix() => Refresh();

			[HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeShakeAccessory), new[] { typeof(int) })]
			internal static void ChaControl_ChangeShakeAccessory_Postfix(ChaControl __instance, int slotNo)
			{
				if (_charaConfigWindow == null) return;
				if (_charaConfigWindow._currentSlotIndex < 0 || _charaConfigWindow._currentSlotIndex != slotNo) return;

				BendUrAccController _pluginCtrl = GetController(__instance);
				if (_pluginCtrl == null) return;

				bool _noShake = JetPack.Accessory.GetPartsInfo(__instance, slotNo).noShake;

				_charaConfigWindow._resetOpenedNodes = false;
				_charaConfigWindow._needRefreshSlotInfo = true;

				if (_noShake)
				{
					__instance.StartCoroutine(ApplySlotBendModifierList());
				}
			}

			private static IEnumerator ApplySlotBendModifierList()
			{
				yield return JetPack.Toolbox.WaitForEndOfFrame;
				yield return JetPack.Toolbox.WaitForEndOfFrame;

				if (_charaConfigWindow._currentSlotModifier.Count > 0)
				{
					GameObject _ca_slot = _charaConfigWindow._selectedParentGameObject;
					foreach (BendModifier _modifier in _charaConfigWindow._currentSlotModifier)
					{
						Transform _node = _ca_slot.transform.Find(_modifier.NodePath);
						if (_node == null)
						{
							_logger.LogWarning($"cannot find node game object {_modifier.Node} {_modifier.NodePath}");
							continue;
						}
						_node.localPosition = _modifier.Position.JsonClone<Vector3>();
						_node.localEulerAngles = _modifier.Rotation.JsonClone<Vector3>();
						_node.localScale = _modifier.Scale.JsonClone<Vector3>();
					}
				}
			}
		}
	}
}