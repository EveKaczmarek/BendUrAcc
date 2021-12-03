using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ChaCustom;

using BepInEx.Configuration;

using JetPack;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal partial class BendUrAccUI : MonoBehaviour
		{
			private ChaControl _chaCtrl => CustomBase.Instance?.chaCtrl;
			private BendUrAccController _pluginCtrl => _chaCtrl?.GetComponent<BendUrAccController>();
			internal bool _onAccTab = false;

			private int _windowRectID;
			private Vector2 _windowSize = new Vector2(510, 540);
			internal Vector2 _windowPos = new Vector2(525, 80);
			private Rect _windowRect, _dragWindowRect;

			private Texture2D _windowBGtex = null;

			private Vector2 _ScreenRes = Vector2.zero;
			private Vector2 _resScaleFactor = Vector2.one;
			private Matrix4x4 _resScaleMatrix;
			private bool _hasFocus = false;
			private bool _initStyle = true;
			internal bool _passThrough = false;

			private readonly GUILayoutOption _gloButtonS = GUILayout.Width(20);
			private readonly GUILayoutOption _gloButtonM = GUILayout.Width(60);
			private readonly GUILayoutOption _gloTextField = GUILayout.Width(50);
			private readonly GUILayoutOption _gloSlider = GUILayout.Width(60);
			private readonly GUILayoutOption _gloItemName = GUILayout.Width(190);

			private GUIStyle _windowSolid;
			private GUIStyle _labelAlignCenter;
			private GUIStyle _labelBoldOrange;
			private GUIStyle _textFieldAlignRight;
			private GUIStyle _textFieldLabelGrey;
			private GUIStyle _textFieldLabel;
			private GUIStyle _buttonActive;

			private float _intervalSliderValue = 2;

			private void Awake()
			{
				DontDestroyOnLoad(this);
				enabled = false;

				_passThrough = _cfgDragPass.Value;
				_windowPos.x = _cfgMakerWinX.Value;
				_windowPos.y = _cfgMakerWinY.Value;
				_windowRect = new Rect(_windowPos.x, _windowPos.y, _windowSize.x, _windowSize.y);
				_windowRectID = GUIUtility.GetControlID(FocusType.Passive);

				_posIncValue = (_cfgPosIncValue.Description.AcceptableValues as AcceptableValueList<float>).AcceptableValues.ToList();
				_rotIncValue = (_cfgRotIncValue.Description.AcceptableValues as AcceptableValueList<float>).AcceptableValues.ToList();
				_sclIncValue = (_cfgSclIncValue.Description.AcceptableValues as AcceptableValueList<float>).AcceptableValues.ToList();
				_posSliderValue = _posIncValue.IndexOf(_cfgPosIncValue.Value);
				_rotSliderValue = _rotIncValue.IndexOf(_cfgRotIncValue.Value);
				_sclSliderValue = _sclIncValue.IndexOf(_cfgSclIncValue.Value);
#if KK
				_windowBGtex = JetPack.UI.MakePlainTex((int) _windowSize.x, (int) _windowSize.y, new Color(0.5f, 0.5f, 0.5f, 1f));
#else
				_windowBGtex = JetPack.UI.MakePlainTex((int) _windowSize.x, (int) _windowSize.y, new Color(0.2f, 0.2f, 0.2f, 1f));
#endif
			}

			private void OnGUI()
			{
				if (!_onAccTab) return;
				if (CustomBase.Instance?.chaCtrl == null) return;
				if (CustomBase.Instance.customCtrl.hideFrontUI) return;
				if (JetPack.Toolbox.SceneIsOverlap()) return;
				if (!JetPack.Toolbox.SceneAddSceneName().IsNullOrEmpty() && JetPack.Toolbox.SceneAddSceneName() != "CustomScene") return;

				if (_ScreenRes.x != Screen.width || _ScreenRes.y != Screen.height)
					ChangeRes();

				if (_initStyle)
				{
					//ChangeRes();

					_windowSolid = new GUIStyle(GUI.skin.window);
					Texture2D _onNormalBG = _windowSolid.onNormal.background;
					_windowSolid.normal.background = _onNormalBG;

					_labelAlignCenter = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

					_textFieldAlignRight = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleRight };

					_textFieldLabel = new GUIStyle(GUI.skin.label);
					_textFieldLabel.wordWrap = false;
					_textFieldLabel.alignment = TextAnchor.MiddleLeft;

					_textFieldLabelGrey = new GUIStyle(GUI.skin.label);
					_textFieldLabelGrey.wordWrap = false;
					_textFieldLabelGrey.alignment = TextAnchor.MiddleLeft;
					_textFieldLabelGrey.normal.textColor = Color.grey;

					_buttonActive = new GUIStyle(GUI.skin.button);
					_buttonActive.normal.textColor = Color.cyan;
					_buttonActive.hover.textColor = Color.cyan;
					_buttonActive.fontStyle = FontStyle.Bold;

					_labelBoldOrange = new GUIStyle(GUI.skin.label);
					_labelBoldOrange.normal.textColor = new Color(1, 0.7f, 0, 1);
					_labelBoldOrange.fontStyle = FontStyle.Bold;

					_initStyle = false;
				}

				GUI.matrix = _resScaleMatrix;

				if (_currentCoordinateIndex != _chaCtrl.fileStatus.coordinateType)
				{
					_currentCoordinateIndex = _chaCtrl.fileStatus.coordinateType;
					_resetOpenedNodes = true;
					_needRefreshSlotInfo = true;
				}
				if (_currentSlotIndex != JetPack.CharaMaker.CurrentAccssoryIndex)
				{
					_currentSlotIndex = JetPack.CharaMaker.CurrentAccssoryIndex;
					_resetOpenedNodes = true;
					_needRefreshSlotInfo = true;
				}

				_dragWindowRect = GUILayout.Window(_windowRectID, _windowRect, DrawWindowContents, "", _windowSolid);
				_windowRect.x = _dragWindowRect.x;
				_windowRect.y = _dragWindowRect.y;

				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = false;

				if ((!_passThrough || _hasFocus) && JetPack.UI.GetResizedRect(_windowRect).Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
					Input.ResetInputAxes();
			}

			private void OnEnable()
			{
				//_hasFocus = true;
			}

			private void OnDisable()
			{
				_initStyle = true;
				_hasFocus = false;
				SetSelectedBone(null);
			}

			private void DrawWindowContents(int _windowID)
			{
				if (_needRefreshSlotInfo)
				{
					ResetAll();

					_searchTerm = _currentSlotIndex < 0 ? "" : $"ca_slot{_currentSlotIndex:00}";

					if (!_searchTerm.IsNullOrEmpty())
					{
						GameObject _ca_slot = JetPack.Accessory.GetObjAccessory(_chaCtrl, _currentSlotIndex);
						if (_ca_slot != null)
						{
							SetSelectedParent(_ca_slot);

							_isAccessoryClothes = _ca_slot.GetComponent(ChaAccessoryClothes) != null;

							_currentSlotModifier = _pluginCtrl.ListSlotModifier(_currentCoordinateIndex, _currentSlotIndex);
							//_currentSlotModifierNodes = _currentSlotModifier?.Count > 0 ? new HashSet<string>(_currentSlotModifier.Select(x => x.Node)) : new HashSet<string>();
							_currentSlotModifierNodes = new HashSet<string>(_currentSlotModifier.Select(x => x.Node));

							_currentSlotChildren.AddRange(_ca_slot.GetComponent<ComponentLookupTable>().Components<Transform>());
						}
					}
				}

#if KKS
				GUI.backgroundColor = Color.grey;
#endif

				Event _windowEvent = Event.current;
				if (EventType.MouseDown == _windowEvent.type || EventType.MouseUp == _windowEvent.type || EventType.MouseDrag == _windowEvent.type || EventType.MouseMove == _windowEvent.type)
					_hasFocus = true;

				GUI.Box(new Rect(0, 0, _windowSize.x, _windowSize.y), _windowBGtex);
				GUI.Box(new Rect(0, 0, _windowSize.x, 30), $"BendUrAcc - Slot{_currentSlotIndex + 1:00}", _labelAlignCenter);

				if (GUI.Button(new Rect(_windowSize.x - 27, 4, 23, 23), new GUIContent("X", "Close this window")))
				{
					CloseWindow();
				}

				if (GUI.Button(new Rect(_windowSize.x - 50, 4, 23, 23), new GUIContent("0", "Config window will not block mouse drag from outside (experemental)"), (_passThrough ? _buttonActive : GUI.skin.button)))
				{
					_passThrough = !_passThrough;
					_cfgDragPass.Value = _passThrough;
					_logger.LogMessage($"Pass through mode: {(_passThrough ? "ON" : "OFF")}");
				}

				if (GUI.Button(new Rect(4, 4, 23, 23), new GUIContent("<", "Reset window position")))
				{
					ResetPos();
				}

				if (GUI.Button(new Rect(27, 4, 23, 23), new GUIContent("T", "Use current window position when reset")))
				{
					_windowPos.x = _dragWindowRect.x;
					_windowPos.y = _dragWindowRect.y;
					_cfgMakerWinX.Value = _windowPos.x;
					_cfgMakerWinY.Value = _windowPos.y;
				}

				GUILayout.BeginVertical();
				{
					GUILayout.Space(10);

					GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
					{
						GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
						{
							DrawGameObjectTree();

							if (JetPack.MoreAccessories.BuggyBootleg)
							{
								GUILayout.BeginHorizontal(GUI.skin.box);
								GUILayout.TextArea("MoreAccessories experimental build detected\nThis version is not meant for production use", _labelBoldOrange);
								GUILayout.EndHorizontal();
							}
						}
						GUILayout.EndVertical();

						GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
						{
							/*
							if (_selectedBonePath.IsNullOrEmpty())
								GUI.enabled = false;
							else
							{
								if (_isDynamicBoneEnabled)
									GUI.enabled = false;
							}
							*/
							GUI.enabled = _enableEdit;
							DrawPosGroup();
							DrawRotGroup();
							DrawSclGroup();
							GUI.enabled = true;

							GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
							{
								GUILayout.Label("Repeat:", GUILayout.Width(50));
								GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
								{
									GUILayout.Space(5);
									_intervalSliderValue = Mathf.Round(GUILayout.HorizontalSlider(_intervalSliderValue, 1, 3, _gloSlider));
								}
								GUILayout.EndVertical();
								GUILayout.Label((_intervalSliderValue / 10).ToString(), _labelAlignCenter, GUILayout.Width(40));
								GUILayout.FlexibleSpace();
							}
							GUILayout.EndHorizontal();
						}
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				GUI.DragWindow();
			}

			private void CloseWindow()
			{
				enabled = false;
			}

			// https://answers.unity.com/questions/840756/how-to-scale-unity-gui-to-fit-different-screen-siz.html
			internal void ChangeRes()
			{
				_ScreenRes.x = Screen.width;
				_ScreenRes.y = Screen.height;
				_resScaleFactor.x = _ScreenRes.x / 1600;
				_resScaleFactor.y = _ScreenRes.y / 900;
				if (_cfgMakerWinResScale.Value)
					_resScaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_resScaleFactor.x, _resScaleFactor.y, 1));
				else
					_resScaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
				ResetPos();
			}

			internal void ResetPos()
			{
				_windowPos.x = _cfgMakerWinX.Value;
				_windowPos.y = _cfgMakerWinY.Value;
				_windowRect.x = _windowPos.x;
				_windowRect.y = _windowPos.y;
			}

			// https://forum.unity.com/threads/repeat-button-speed.132477/
			private bool _controlbool = false;
			private IEnumerator WaitCor()
			{
				yield return new WaitForSeconds(_intervalSliderValue / 10);
				_controlbool = false;
			}
		}
	}
}