.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1438353025
	.compileTime 1438353025
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
.object PDN_DateTime
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function get_Now static
					.userFlags 0
					.docString ""
					.return Float
					.paramTable
					.endParamTable
					.localTable
						.local V_0 Float
					.endLocalTable
					.code
						CallStatic Utility GetCurrentRealTime V_0 
						Jump _label9
					_label9:
						Return V_0
					.endCode
				.endFunction
				.function get_Min static
					.userFlags 0
					.docString ""
					.return Float
					.paramTable
					.endParamTable
					.localTable
						.local V_0 Float
					.endLocalTable
					.code
						Assign V_0 0
						Jump _label13
					_label13:
						Return V_0
					.endCode
				.endFunction
				.function get_Max static
					.userFlags 0
					.docString ""
					.return Float
					.paramTable
					.endParamTable
					.localTable
						.local V_0 Float
					.endLocalTable
					.code
						Assign V_0 99999999999
						Jump _label13
					_label13:
						Return V_0
					.endCode
				.endFunction
				.function OnInit
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
					.endLocalTable
					.code
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable