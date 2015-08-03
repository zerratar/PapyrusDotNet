.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1438617588
	.compileTime 1438617588
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
.object PDN_Random
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function RangeInt static
					.userFlags 0
					.docString ""
					.return Int
					.paramTable
						.param min Int
						.param max Int
					.endParamTable
					.localTable
						.local V_0 Int
					.endLocalTable
					.code
						CallStatic Utility RandomInt V_0 min max
						Jump _label11
					_label11:
						Return V_0
					.endCode
				.endFunction
				.function Range static
					.userFlags 0
					.docString ""
					.return Float
					.paramTable
						.param min Float
						.param max Float
					.endParamTable
					.localTable
						.local V_0 Float
					.endLocalTable
					.code
						CallStatic Utility RandomFloat V_0 min max
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