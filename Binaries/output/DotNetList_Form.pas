.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1391435453
	.compileTime 1391435453
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
	.object DotNetList_Form 
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::Capacity Int
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::Length Int
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::Count Int
				.userFlags 0
				.initialValue None
			.endVariable
			.variable ::Arrays DotNetListStack_Form[]
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function OnInit
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
					.endLocalTable
					.code
						Assign ::Length 0
						Assign ::Count 0
						Assign ::Capacity 16384
						Assign ::Capacity 16384
						Return None
					.endCode
				.endFunction
				.function ArrayIndex
					.userFlags 0
					.docString ""
					.return Int
					.paramTable
						.param bigIndex Int
					.endParamTable
					.localTable
						.local V_0 Int
					.endLocalTable
					.code
						IDivide V_0 bigIndex 128
						Jump _label11
					_label11:
						Return V_0
					.endCode
				.endFunction
				.function Add
					.userFlags 0
					.docString ""
					.return None
					.paramTable
						.param item Form
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Int
						.local V_1 Int
						.local ::temp0 DotNetListItem_Form
					.endLocalTable
					.code
						IDivide V_0 ::Length 128
						IMultiply V_1 V_0 128
						CallMethod set_Item item ::NoneVar ::Length item
						CallMethod get_Item item ::temp0 V_1
						Assign V_0 ::temp0
						IAdd ::Length ::Length 1
						IAdd ::Count ::Count 1
						Return None
					.endCode
				.endFunction
				.function Remove
					.userFlags 0
					.docString ""
					.return None
					.paramTable
						.param item Form
					.endParamTable
					.localTable
						.local V_0 DotNetListItem_Form
						.local V_1 Int
						.local V_2 Int
						.local V_3 Bool
						.local ::temp0 Int
					.endLocalTable
					.code
						Assign V_0 None
						Assign V_0 V_0
						Assign V_1 0
						Jump _label114
					_label18:
						Assign V_2 0
						Jump _label82
					_label23:
						ArrayGetElement V_3 ::Arrays V_1
						JumpF V_3 _label82
						ArrayGetElement V_2 ::Arrays V_1
					_label82:
						ArrayGetElement V_3 ::Arrays V_1
						JumpF V_3 _label23
						IAdd V_1 V_1 1
					_label114:
						ArrayLength ::temp0 ::Arrays
						Cast V_3 item
						CompareLT V_3 item ::temp0
						JumpF V_3 _label18
						Return None
					.endCode
				.endFunction
				.function RemoveAt
					.userFlags 0
					.docString ""
					.return None
					.paramTable
						.param index Int
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Int
						.local V_1 Int
					.endLocalTable
					.code
						IDivide V_0 index 128
						IMultiply V_1 V_0 128
						CallMethod set_Item index ::NoneVar V_1
						ISubtract ::Count ::Count 1
						Return None
					.endCode
				.endFunction
				.function get_Item
					.userFlags 0
					.docString ""
					.return Form
					.paramTable
						.param index Int
					.endParamTable
					.localTable
						.local V_0 Int
						.local V_1 Int
						.local V_2 Int
						.local V_3 Form
					.endLocalTable
					.code
						CallMethod ArrayIndex index V_0 index
						IMultiply V_1 V_0 128
						ISubtract V_2 index V_1
						ArrayGetElement V_3 ::Arrays V_0
						Jump _label44
					_label44:
						Return V_3
					.endCode
				.endFunction
				.function set_Item
					.userFlags 0
					.docString ""
					.return None
					.paramTable
						.param index Int
						.param value Form
					.endParamTable
					.localTable
						.local V_0 Int
						.local V_1 Int
						.local V_2 Int
					.endLocalTable
					.code
						CallMethod ArrayIndex index V_0 index
						IMultiply V_1 V_0 128
						ISubtract V_2 index V_1
						Assign V_0 None
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable