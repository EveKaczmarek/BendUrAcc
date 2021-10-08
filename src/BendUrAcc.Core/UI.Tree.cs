using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using HarmonyLib;

using JetPack;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal partial class BendUrAccUI
		{
			private Vector2 _parentListScrollPos = Vector2.zero;

			internal bool _needRefreshSlotInfo = true;
			internal int _currentCoordinateIndex = -1;
			internal int _currentSlotIndex = -1;
			internal List<BendModifier> _currentSlotModifier = new List<BendModifier>();
			internal Transform _boneInicator = null;
			private bool _cfgShowInicator = true;

			internal void ResetAll()
			{
				_enableEdit = false;

				_searchTerm = "";

				if (_resetOpenedNodes)
				{
					_openedNodes.Clear();
					_resetOpenedNodes = false;
				}

				_currentSlotModifier.Clear();
				_currentSlotModifierNodes.Clear();

				_isAccessoryClothes = false;
				_cmpDynamicBone = null;

				_currentNodeModifier = null;

				_selectedBoneGameObject = null;
				_selectedParentGameObject = null;

				_selectedBonePath = "";
				_selectedParentPath = "";

				_needRefreshSlotInfo = false;
			}

			internal void MoveWithMe()
			{
				if (_isDynamicBoneEnabled)
					return;

				if (_currentNodeModifier == null)
				{
					BendModifier _modifier = new BendModifier();
					_modifier.Coordinate = _currentCoordinateIndex;
					_modifier.Slot = _currentSlotIndex;
					_modifier.SlotType = SlotType.Accessory;
					_modifier.Node = _selectedBoneGameObject.name;
					_modifier.NodePath = _selectedBonePath.Replace(_selectedParentPath + "/", "");

					_pluginCtrl.BendModifierList.Add(_modifier);
					_currentNodeModifier = _pluginCtrl.GetModifier(_currentCoordinateIndex, _currentSlotIndex, _selectedBoneGameObject.name);
					_currentSlotModifierNodes.Add(_selectedBoneGameObject.name);
				}
				_currentNodeModifier.Position = _selectedBoneGameObject.transform.localPosition.JsonClone<Vector3>();
				_currentNodeModifier.Rotation = _selectedBoneGameObject.transform.localEulerAngles.JsonClone<Vector3>();
				_currentNodeModifier.Scale = _selectedBoneGameObject.transform.localScale.JsonClone<Vector3>();
			}

			private void DrawGameObjectTree()
			{
				GUILayout.BeginHorizontal(GUILayout.Width(300), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
				{
					GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
					{
						if (_isAccessoryClothes)
							GUI.enabled = false;

						_parentListScrollPos = GUILayout.BeginScrollView(_parentListScrollPos, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box);
						{
							if (!_searchTerm.IsNullOrEmpty())
								BuildObjectTree(Root, 0);
						}
						GUILayout.EndScrollView();

						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						{
							GUILayout.Label("Remove:");

							GUILayout.FlexibleSpace();

							if (GUILayout.Button("Slot", _gloButtonM))
							{
								_pluginCtrl.RemoveSlotModifier(_currentCoordinateIndex, _currentSlotIndex);
								_needRefreshSlotInfo = true;
							}

							if (_currentNodeModifier == null)
								GUI.enabled = false;
							if (GUILayout.Button("Node", _gloButtonM))
							{
								_pluginCtrl.RemoveModifier(_currentCoordinateIndex, _currentSlotIndex, _selectedBoneGameObject.name);
								_needRefreshSlotInfo = true;
							}
							GUI.enabled = true;
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUI.skin.box);
						{
							bool _showIndicator = _cfgShowInicator;
							if (_showIndicator != GUILayout.Toggle(_cfgShowInicator, new GUIContent(" indicator", "Show a indicator of selected node")))
							{
								if (_boneInicator == null)
								{
									_cfgShowInicator = false;
								}
								else
								{
									_cfgShowInicator = !_boneInicator.gameObject.activeSelf;
									_boneInicator.gameObject.SetActive(_cfgShowInicator);
								}
							}

							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUI.skin.box);
						GUILayout.Label(GUI.tooltip);
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}

			internal HashSet<string> _currentSlotModifierNodes = new HashSet<string>();
			internal BendModifier _currentNodeModifier = null;

			internal bool _isAccessoryClothes = false;
			internal DynamicBone _cmpDynamicBone = null;

			internal string _searchTerm = "";
			internal HashSet<GameObject> _openedNodes = new HashSet<GameObject>();
			internal bool _resetOpenedNodes = false;

			internal GameObject _selectedBoneGameObject = null;
			internal GameObject _selectedParentGameObject = null;

			internal string _selectedBonePath = "";
			internal string _selectedParentPath = "";

			internal bool _enableEdit = false;

			internal bool _isDynamicBoneEnabled
			{
				get
				{
					if (_cmpDynamicBone != null && _cmpDynamicBone.enabled)
						return true;
					return false;
				}
			}

			private static HashSet<string> _ignoreList = new HashSet<string>() { _boneInicatorName, "AAAPK_indicator" };

			private void BuildObjectTree(GameObject _gameObject, int indentLevel)
			{
				if (_ignoreList.Contains(_gameObject.name) || _gameObject.name.StartsWith("AccGotHigh_")) return;

				if (_searchTerm.Length == 0 || _gameObject.name.IndexOf(_searchTerm, StringComparison.OrdinalIgnoreCase) > -1 || _openedNodes.Contains(_gameObject.transform.parent.gameObject))
				{
					Color _color = GUI.color;
					if (_selectedBoneGameObject == _gameObject)
						GUI.color = Color.cyan;
					else if (_currentSlotModifierNodes?.Count > 0 && _currentSlotModifierNodes.Contains(_gameObject.name))
						GUI.color = Color.magenta;

					GUILayout.BeginHorizontal();

					if (_openedNodes.Contains(_gameObject.transform.parent.gameObject))
						GUILayout.Space(indentLevel * 25f);
					else
						indentLevel = 0;

					if (_gameObject.transform.childCount > 0 && _gameObject.transform.Cast<Transform>().Where(x => _ignoreList.Contains(x.name)).Count() == _gameObject.transform.childCount)
					{
						GUILayout.Space(19);
					}
					else if (_gameObject.transform.childCount > 0)
					{
						if (GUILayout.Toggle(_openedNodes.Contains(_gameObject), "", GUILayout.ExpandWidth(false)))
							_openedNodes.Add(_gameObject);
						else
							_openedNodes.Remove(_gameObject);
					}
					else
						GUILayout.Space(19);

					if (_isAccessoryClothes)
					{
						GUI.enabled = false;
						GUILayout.Button(new GUIContent(_gameObject.name, "This is used by Accessory Clothes, do not edit"), GUILayout.ExpandWidth(false));
						GUI.enabled = true;
					}
					else if (_gameObject.GetComponent<ListInfoComponent>() != null && _gameObject.name.StartsWith("ca_slot"))
					{
						GUI.enabled = false;
						GUILayout.Button(new GUIContent(_gameObject.name, "ca_slot is used for PartsInfo, do not edit"), GUILayout.ExpandWidth(false));
						GUI.enabled = true;
					}
					else if (_gameObject.GetComponentsInParent<ListInfoComponent>(true).FirstOrDefault().name != _selectedParentGameObject.name)
					{
						GUI.enabled = false;
						GUILayout.Button(new GUIContent(_gameObject.name, "This is from another slot, do not edit"), GUILayout.ExpandWidth(false));
						GUI.enabled = true;
					}
					else if (_gameObject.name.StartsWith("N_move") || _gameObject.name.StartsWith("o_root"))
					{
						GUI.enabled = false;
						GUILayout.Button(new GUIContent(_gameObject.name, "This GameObject is used for PartsInfo, do not edit"), GUILayout.ExpandWidth(false));
						GUI.enabled = true;
					}
					/*
					else if (_gameObject.GetComponent<Renderer>() != null || _gameObject.GetComponentsInParent<Renderer>(true)?.Length > 0)
					{
						GUI.enabled = false;
						GUILayout.Button(new GUIContent(_gameObject.name, "This GameObject is used by a Renderer, do not edit"), GUILayout.ExpandWidth(false));
						GUI.enabled = true;
					}
					*/
					else
					{
						if (GUILayout.Button(_gameObject.name, GUILayout.ExpandWidth(false)))
						{
							_enableEdit = true;
							_cmpDynamicBone = null;
							List<DynamicBone> _cmps = _selectedParentGameObject.GetComponents<DynamicBone>()?.Where(x => x.m_Root != null).ToList();
							if (_cmps?.Count > 0)
							{
								foreach (DynamicBone _cmp in _cmps)
								{
									if ((bool) _cmp.m_Particles.Where(x => x!= null && x?.m_Transform != null)?.Any(x => x.m_Transform == _gameObject.transform))
									{
										_cmpDynamicBone = _cmp;
										break;
									}
								}
							}

							if (_isDynamicBoneEnabled)
							{
								_enableEdit = false;
								_logger.LogMessage($"{_gameObject.name} is locked for editing because it is affacted by DynamicBone");
							}

							SetSelectedBone(_gameObject);
							_currentNodeModifier = _pluginCtrl.GetModifier(_currentCoordinateIndex, _currentSlotIndex, _selectedBoneGameObject.name);
						}
					}

					GUILayout.EndHorizontal();
					GUI.color = _color;
				}

				if (_searchTerm.Length > 0 || _openedNodes.Contains(_gameObject))
				{
					foreach (Transform child in _gameObject.transform)
						BuildObjectTree(child.gameObject, indentLevel + 1);
				}
			}

			private GameObject Root
			{
				get
				{
					return _chaCtrl.objAnim.transform.Find("cf_j_root").gameObject;
					//return _chaCtrl.transform.Find("BodyTop").gameObject;
				}
			}

			internal void SetSelectedBone(GameObject _gameObject)
			{
				_selectedBoneGameObject = _gameObject;
				_selectedBonePath = BuildParentString(_gameObject);

				if (_gameObject == null)
				{
					if (_boneInicator != null)
						Destroy(_boneInicator.gameObject);
				}
				else
				{
					if (_boneInicator == null)
					{
						Transform _copy = Instantiate(_assetSphere.transform, _selectedBoneGameObject.transform, false);
						if (_copy != null)
						{
							_copy.name = _boneInicatorName;
							_boneInicator = _copy;
						}
					}
					else
					{
						_boneInicator.SetParent(_selectedBoneGameObject.transform, false);
					}
					_boneInicator.gameObject.SetActive(_cfgShowInicator);
				}
			}

			internal void SetSelectedParent(GameObject _gameObject)
			{
				_selectedParentGameObject = _gameObject;
				_selectedParentPath = BuildParentString(_gameObject);
			}

			private string BuildParentString(GameObject _gameObject)
			{
				if (_gameObject == null)
					return "";

				string _fullPath = _gameObject.name;
				GameObject _current = _gameObject.transform.parent.gameObject;
				while (_current != _chaCtrl.gameObject)
				{
					_fullPath = _current.name + "/" + _fullPath;
					_current = _current.transform.parent.gameObject;
				};
				return _fullPath;
			}

			private List<GameObject> GetParents(GameObject _gameObject, GameObject _top)
			{
				List<GameObject> _parents = new List<GameObject>();

				if (_gameObject == null)
					return _parents;

				GameObject _current = _gameObject.transform.parent.gameObject;
				while (_current != _top.gameObject)
				{
					_parents.Add(_current);
					_current = _current.transform.parent.gameObject;
				};
				return _parents;
			}
		}
	}
}