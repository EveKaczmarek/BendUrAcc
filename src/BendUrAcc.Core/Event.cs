using KKAPI.Chara;

namespace BendUrAcc
{
	public partial class BendUrAcc
	{
		internal static void OnChangeCoordinateType(JetPack.Chara.ChangeCoordinateTypeEventArgs _args)
		{
			if (_args.State == "Prefix")
				OnChangeCoordinateType_Prefix(_args.ChaControl);
			else if (_args.State == "Coroutine")
				OnChangeCoordinateType_Coroutine(_args.ChaControl);
		}

		internal static void OnChangeCoordinateType_Prefix(ChaControl _chaCtrl)
		{
			BendUrAccController _pluginCtrl = GetController(_chaCtrl);
			if (_pluginCtrl == null) return;

			_pluginCtrl._duringLoadChange = true;
			if (_charaConfigWindow != null)
				_charaConfigWindow.ResetAll();
		}

		internal static void OnChangeCoordinateType_Coroutine(ChaControl _chaCtrl)
		{
			BendUrAccController _pluginCtrl = GetController(_chaCtrl);
			if (_pluginCtrl == null) return;

			_pluginCtrl._duringLoadChange = false;
			_pluginCtrl.RefreshCache();
		}

		internal static void OnDataApply(JetPack.MaterialEditor.ControllerEventArgs _args)
		{
			if (_args.State != "Coroutine") return;

			BendUrAccController _pluginCtrl = GetController((_args.Controller as CharaCustomFunctionController).ChaControl);
			if (_pluginCtrl == null) return;

			_pluginCtrl.ApplyBendModifierList("OnDataApply");
		}
	}
}
