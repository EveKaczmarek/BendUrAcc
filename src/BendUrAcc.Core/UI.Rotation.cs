using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal partial class BendUrAccUI
		{
			private float _rotX, _rotY, _rotZ;
			private float _rotSliderValue = 1;
			private List<float> _rotIncValue = new List<float>();

			private void DrawRotGroup()
			{
				GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				{
					Vector3 _rot = _selectedBoneGameObject == null ? Vector3.zero : _selectedBoneGameObject.transform.localEulerAngles;
					float _inc = _rotIncValue[(int)_rotSliderValue];

					GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
					{
						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						GUILayout.Label("Rotation", _labelAlignCenter);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" X:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.x = (_rot.x - _inc + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_rotX = _rot.x;
							if (float.TryParse(GUILayout.TextField(_rotX.ToString("f2", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _rotX))
							{
								/*
								if (_rot.x != _rotX)
								{
									_rot.x = (_rotX + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.x = (_rot.x + _inc + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_rot.x = 0;
								_selectedBoneGameObject.transform.localEulerAngles = _rot;
								MoveWithMe();
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(2);
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" Y:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.y = (_rot.y - _inc + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_rotY = _rot.y;
							if (float.TryParse(GUILayout.TextField(_rotY.ToString("f2", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _rotY))
							{
								/*
								if (_rot.y != _rotY)
								{
									_rot.y = (_rotY + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.y = (_rot.y + _inc + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_rot.y = 0;
								_selectedBoneGameObject.transform.localEulerAngles = _rot;
								MoveWithMe();
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(2);
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" Z:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.z = (_rot.z - _inc + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_rotZ = _rot.z;
							if (float.TryParse(GUILayout.TextField(_rotZ.ToString("f2", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _rotZ))
							{
								/*
								if (_rot.z != _rotZ)
								{
									_rot.z = (_rotZ + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_rot.z = (_rot.z + _inc + 360f) % 360f;
									_selectedBoneGameObject.transform.localEulerAngles = _rot;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_rot.z = 0;
								_selectedBoneGameObject.transform.localEulerAngles = _rot;
								MoveWithMe();
							}
						}
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						{
							GUILayout.Space(5);
							GUILayout.Label("Inc:", GUILayout.Width(40));
							GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
							{
								GUILayout.Space(5);
								_rotSliderValue = Mathf.Round(GUILayout.HorizontalSlider(_rotSliderValue, 0, _rotIncValue.Count - 1, _gloSlider));
							}
							GUILayout.EndVertical();
							GUILayout.Label(_inc.ToString(), _labelAlignCenter, GUILayout.Width(40));
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
		}
	}
}