using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal partial class BendUrAccUI
		{
			private float _sclX, _sclY, _sclZ;
			private float _sclSliderValue = 1;
			private List<float> _sclIncValue = new List<float>();

			private void DrawSclGroup()
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				{
					Vector3 _scl = _selectedBoneGameObject == null ? Vector3.one : _selectedBoneGameObject.transform.localScale;
					float _inc = _sclIncValue[(int) _sclSliderValue];

					GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false));
					{
						GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
						GUILayout.Label("Scale", _labelAlignCenter);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
						{
							GUILayout.Label(" X:");

							if (GUILayout.RepeatButton("<", _gloButtonS))
							{
								if (!_controlbool)
								{
									_scl.x -= _inc;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_sclX = _scl.x;
							if (float.TryParse(GUILayout.TextField(_sclX.ToString("f3", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _sclX))
							{
								/*
								if (_scl.x != _sclX)
								{
									_scl.x = _sclX;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_scl.x += _inc;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_scl.x = 1;
								_selectedBoneGameObject.transform.localScale = _scl;
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
									_scl.y -= _inc;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_sclY = _scl.y;
							if (float.TryParse(GUILayout.TextField(_sclY.ToString("f3", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _sclY))
							{
								/*
								if (_scl.y != _sclY)
								{
									_scl.y = _sclY;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_scl.y += _inc;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_scl.y = 1;
								_selectedBoneGameObject.transform.localScale = _scl;
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
									_scl.z -= _inc;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							_sclZ = _scl.z;
							if (float.TryParse(GUILayout.TextField(_sclZ.ToString("f3", CultureInfo.InvariantCulture), _textFieldAlignRight, _gloTextField), out _sclZ))
							{
								/*
								if (_scl.z != _sclZ)
								{
									_scl.z = _sclZ;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
								}
								*/
							}

							if (GUILayout.RepeatButton(">", _gloButtonS))
							{
								if (!_controlbool)
								{
									_scl.z += _inc;
									_selectedBoneGameObject.transform.localScale = _scl;
									MoveWithMe();
									_controlbool = true;
									StartCoroutine(WaitCor());
								}
							}

							if (GUILayout.Button("0", _gloButtonS))
							{
								_scl.z = 1;
								_selectedBoneGameObject.transform.localScale = _scl;
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
								_sclSliderValue = Mathf.Round(GUILayout.HorizontalSlider(_sclSliderValue, 0, _sclIncValue.Count - 1, _gloSlider));
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