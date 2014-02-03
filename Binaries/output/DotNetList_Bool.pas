.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1391440704
	.compileTime 1391440704
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
	.object DotNetList_Bool 
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
			.variable ::Arrays DotNetListStack_Bool[]
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
						ArrayCreate ::Arrays 128
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
						.param item Bool
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Int
						.local V_1 Int
					.endLocalTable
					.code
						IDivide V_0 ::Length 128
						IMultiply V_1 V_0 128
						ArrayGetElement V_0 ::Arrays V_0
						IAdd ::Length ::Length 1
						IAdd ::Count ::Count 1
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
						ArrayGetElement V_0 ::Arrays V_0
						Return None
					.endCode
				.endFunction
				.function get_Item
					.userFlags 0
					.docString ""
					.return Bool
					.paramTable
						.param index Int
					.endParamTable
					.localTable
						.local V_0 Int
						.local V_1 Int
						.local V_2 Int
						.local V_3 Bool
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
						.param value Bool
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
						ArrayGetElement V_0 ::Arrays V_0
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable