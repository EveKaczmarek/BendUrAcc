using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal partial class BendUrAccUI
		{
			private float _posX, _posY, _posZ;
			private float _posSliderValue = 2;
			private List<float> _posIncValue = new List<float>();

			private void DrawPosGroup()
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				{
					Vector3 _pos = _selectedBoneGameObject == null ? Vector3.zero : _selectedBoneGameObject.transform.localPosition;
					float _inc = _posIncValue[(int) _posSliderValue];

					GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false));
					{
						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						GUILayout.Label("Position", _labelAlignCenter);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" X:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.x -= _inc;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_posX = _pos.x;
							if (float.TryParse(GUILayout.TextField(_posX.ToString("f3", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _posX))
							{
								/*
								if (_pos.x != _posX)
								{
									_pos.x = _posX;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.x += _inc;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_pos.x = 0;
								_selectedBoneGameObject.transform.localPosition = _pos;
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
									_pos.y -= _inc;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_posY = _pos.y;
							if (float.TryParse(GUILayout.TextField(_posY.ToString("f3", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _posY))
							{
								/*
								if (_pos.y != _posY)
								{
									_pos.y = _posY;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.y += _inc;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_pos.y = 0;
								_selectedBoneGameObject.transform.localPosition = _pos;
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
									_pos.z -= _inc;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_posZ = _pos.z;
							if (float.TryParse(GUILayout.TextField(_posZ.ToString("f3", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _posZ))
							{
								/*
								if (_pos.z != _posZ)
								{
									_pos.z = _posZ;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_pos.z += _inc;
									_selectedBoneGameObject.transform.localPosition = _pos;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_pos.z = 0;
								_selectedBoneGameObject.transform.localPosition = _pos;
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
								_posSliderValue = Mathf.Round(GUILayout.HorizontalSlider(_posSliderValue, 0, _posIncValue.Count - 1, _gloSlider));
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