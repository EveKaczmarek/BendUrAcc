using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using MessagePack;
using ParadoxNotion.Serialization;

using ExtensibleSaveFormat;

using JetPack;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal static string ExtDataKey = "BendUrAcc";
		internal static int ExtDataVer = 1;

		public enum SlotType
		{
			Unknown,
			Clothing,
			Accessory,
			Hair,
			Character,
		}

		[Serializable]
		[MessagePackObject]
		public class BendModifier
		{
			[Key("Coordinate")]
			public int Coordinate { get; set; }
			[Key("Slot")]
			public int Slot { get; set; }
			[Key("SlotType")]
			public SlotType SlotType { get; set; }
			[Key("Node")]
			public string Node { get; set; }
			[Key("NodePath")]
			public string NodePath { get; set; }
			[Key("Position")]
			public Vector3 Position { get; set; } = Vector3.zero;
			[Key("Rotation")]
			public Vector3 Rotation { get; set; } = Vector3.zero;
			[Key("Scale")]
			public Vector3 Scale { get; set; } = Vector3.one;
		}
#if KKS
		internal static void InitCardImport()
		{
			ExtendedSave.CardBeingImported += CardBeingImported;
		}

		internal static void CardBeingImported(Dictionary<string, PluginData> _importedExtData, Dictionary<int, int?> _coordinateMapping)
		{
			if (_importedExtData.TryGetValue(ExtDataKey, out PluginData _pluginData))
			{
				List<BendModifier> BendModifiers = new List<BendModifier>();

				if (_pluginData != null)
				{
					if (_pluginData.data.TryGetValue("BendModifierList", out object _loadedBendModifiers) && _loadedBendModifiers != null)
					{
						List<BendModifier> _tempBendModifiers = MessagePackSerializer.Deserialize<List<BendModifier>>((byte[]) _loadedBendModifiers);
						if (_tempBendModifiers?.Count > 0)
						{
							for (int i = 0; i < _coordinateMapping.Count; i++)
							{
								if (_coordinateMapping[i] == null) continue;

								List<BendModifier> _copy = _tempBendModifiers.Where(x => x.Coordinate == i).ToList().JsonClone<List<BendModifier>>();
								if (_copy.Count == 0) continue;

								_copy.ForEach(x => x.Coordinate = (int) _coordinateMapping[i]);
								BendModifiers.AddRange(_copy);
							}
						}
					}
				}

				_importedExtData.Remove(ExtDataKey);

				if (BendModifiers?.Count > 0)
				{
					PluginData _pluginDataNew = new PluginData() { version = ExtDataVer };
					_pluginDataNew.data.Add("BendModifierList", MessagePackSerializer.Serialize(BendModifiers));
					_importedExtData[ExtDataKey] = _pluginDataNew;
				}
			}
		}
#endif
	}
}
